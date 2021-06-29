// <copyright file="manage-events.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import moment from "moment";
import { Flex, Table, Button, Input, Text, TableRowProps, Loader, SplitButton, MenuItemProps, List, CloseIcon } from "@fluentui/react-northstar";
import { AddIcon, SearchIcon, MoreIcon } from "@fluentui/react-northstar";
import TabMenu from "../tab-menu/tab-menu";
import ManageEventsMenu from "./manage-events-menu";
import { IEvent } from "../../models/IEvent";
import { EventSearchType } from "../../models/event-search-type";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { getEventsAsync, exportEventDetailsToCSV, sendReminder } from "../../api/manage-events-api";
import Constants, { ResponseStatus } from "../../constants/constants";
import NoContent from "../no-content-page/no-content-page"
import InfiniteScroll from "react-infinite-scroller";
import { EventType } from "../../models/event-type";
import { EventStatus } from "../../models/event-status";
import { IToastNotification } from "../../models/IToastNotification";
import ToastNotification from "../toast-notification/toast-notification";
import { ActivityStatus } from "../../models/activity-status";
import { EventOperationType } from "../../models/event-operation-type";
import withContext, { IWithContext } from "../../providers/context-provider";
import { Fabric } from "@fluentui/react";
import { LanguageDirection } from "../../models/language-direction";

import "../manage-events/manage-events.css";

const DraftEventsTabIndex: number = 0;
const ActiveEventsTabIndex: number = 1;
const CompletedEventsTabIndex: number = 2;

interface IManageEventsState {
    activeTabIndex: number | string,
    draftEvents: Array<IEvent>,
    activeEvents: Array<IEvent>,
    completedEvents: Array<IEvent>,
    isLoading: boolean,
    infiniteScrollKey: number,
    loadMoreEvents: boolean,
    isInitialLoadComplete: boolean,
    isMobileView: boolean,
    notification: IToastNotification,
    searchText: string,
    isMobileSearchBoxOpen: boolean
}

interface IManageEventsProps extends WithTranslation, IWithContext {
}

/** This class manages all the events created by LnD team */
class ManageEvents extends React.Component<IManageEventsProps, IManageEventsState> {
    readonly localize: TFunction;
    draftEventsCount: number;
    completedEventsCount: number;
    activeEventsCount: number;
    teamId: string;
    timeout: number | null;
    eventsScrollAreaRef: any;

    constructor(props: any) {
        super(props);

        this.localize = this.props.t;
        this.teamId = "";
        this.timeout = null;
        this.draftEventsCount = -1;
        this.completedEventsCount = -1;
        this.activeEventsCount = -1;
        this.eventsScrollAreaRef = null;

        this.state = {
            activeTabIndex: ActiveEventsTabIndex,
            draftEvents: [],
            activeEvents: [],
            completedEvents: [],
            isLoading: true,
            infiniteScrollKey: 0,
            loadMoreEvents: false,
            isInitialLoadComplete: false,
            isMobileView: false,
            notification: { id: 0, message: "", type: ActivityStatus.None },
            searchText: "",
            isMobileSearchBoxOpen: false
        }
    }

    /**
     * Gets teams context from HOC as props
     */
    componentWillReceiveProps= async (nextProps: IManageEventsProps) => {
        if (this.props.teamsContext !== nextProps.teamsContext) {
            if (nextProps.teamsContext) {
                this.teamId = nextProps.teamsContext.teamId!;

                if (this.activeEventsCount === -1 && this.draftEventsCount === -1 && this.completedEventsCount === -1) {
                    this.activeEventsCount = this.draftEventsCount = this.completedEventsCount = 0;

                    let activeEvents = await this.getEventsAsync(0, EventSearchType.ActiveEventsForTeam);
                    let draftEvents = await this.getEventsAsync(0, EventSearchType.DraftEventsForTeam);
                    let completedEvents = await this.getEventsAsync(0, EventSearchType.CompletedEventsForTeam);

                    this.setTotalEventsCount(EventSearchType.ActiveEventsForTeam, activeEvents.length);
                    this.setTotalEventsCount(EventSearchType.DraftEventsForTeam, draftEvents.length);
                    this.setTotalEventsCount(EventSearchType.CompletedEventsForTeam, completedEvents.length);

                    this.getEvents();
                }
            }
        }
    }

    componentDidMount() {
        window.addEventListener("resize", this.onScreenSizeChange.bind(this));
        this.onScreenSizeChange();
    }

    /** The HTTP GET call to get events of LnD team based on event status */
    getEventsAsync = async (pageNumber: number, eventSearchType: EventSearchType) => {
        let response = await getEventsAsync(this.state.searchText, pageNumber, eventSearchType, this.teamId);

        let events: Array<IEvent> = [];

        if (response && response.status === ResponseStatus.OK && response.data) {
            events = response.data;
        }

        return events;
    }

    /** Loads the events details on UI */
    loadEvents = async (pageNumber: number, eventSearchType: EventSearchType) => {
        let events: Array<IEvent> = await this.getEventsAsync(pageNumber, eventSearchType);

        if (events) {
            let loadMoreEvents: boolean = events.length < Constants.lazyLoadEventsCount ? false : true;

            switch (eventSearchType) {
                case EventSearchType.DraftEventsForTeam:
                    if (pageNumber === 0) {
                        this.setTotalEventsCount(EventSearchType.DraftEventsForTeam, events.length);
                        this.setState({ draftEvents: [...events], loadMoreEvents });
                    }
                    else {
                        let draftEvents = [...this.state.draftEvents];
                        this.setState({ draftEvents: [...draftEvents, ...events], loadMoreEvents });
                    }
                    break;

                case EventSearchType.CompletedEventsForTeam:
                    if (pageNumber === 0) {
                        this.setTotalEventsCount(EventSearchType.CompletedEventsForTeam, events.length);
                        this.setState({ completedEvents: [...events], loadMoreEvents });
                    }
                    else {
                        let completedEvents = [...this.state.completedEvents];
                        this.setState({ completedEvents: [...completedEvents, ...events], loadMoreEvents });
                    }
                    break;

                default:
                    if (pageNumber === 0) {
                        this.setTotalEventsCount(EventSearchType.ActiveEventsForTeam, events.length);
                        this.setState({ activeEvents: [...events], loadMoreEvents });
                    }
                    else {
                        let activeEvents = [...this.state.activeEvents];
                        this.setState({ activeEvents: [...activeEvents, ...events], loadMoreEvents });
                    }
                    break;
            }
        }
        else {
            if (pageNumber === 0) {
                this.setTotalEventsCount(eventSearchType, 0);
            }

            this.setState({ loadMoreEvents: false });
        }
    }

    /**
     * Sets the total event count for selected tab
     * @param count The total count to set
     */
    setTotalEventsCount = (eventSeatchType: EventSearchType, count: number) => {
        if (!this.state.searchText || this.state.searchText.trim() === "") {
            switch (eventSeatchType) {
                case EventSearchType.DraftEventsForTeam:
                    this.draftEventsCount = count;
                    break;

                case EventSearchType.CompletedEventsForTeam:
                    this.completedEventsCount = count;
                    break;

                case EventSearchType.ActiveEventsForTeam:
                    this.activeEventsCount = count;
                    break;

                default:
                    break;
            }
        }
    }

    /**
     * Gets events based on currently selected tab
     * @param pageNumber The page number for which events to get
     */
    getEvents = (pageNumber: number = 0) => {
        if (pageNumber === 0) {
            this.setState((prevState: IManageEventsState) =>
                ({ isLoading: true, loadMoreEvents: false, infiniteScrollKey: prevState.infiniteScrollKey + 1 }));
        }

        switch (this.state.activeTabIndex) {
            case DraftEventsTabIndex:
                this.loadEvents(pageNumber, EventSearchType.DraftEventsForTeam)
                    .finally(() => {
                        this.setState({ isLoading: false });
                    });
                break;

            case CompletedEventsTabIndex:
                this.loadEvents(pageNumber, EventSearchType.CompletedEventsForTeam)
                    .finally(() => {
                        this.setState({ isLoading: false });
                    });
                break;

            default:
                this.loadEvents(pageNumber, EventSearchType.ActiveEventsForTeam)
                    .finally(() => {
                        this.setState({ isLoading: false });
                    });
                break;
        }
    }

    /**
     * Formats evetn's date and time in the format- {Date} {Start Time}-{End Time} => 05 Jun 2020, 13:00 - 16:00
     * @param eventDetails
     */
    formatEventDateAndTime = (eventDetails: IEvent) => {
        if (eventDetails) {
            let eventDayMonthYear = moment.utc(eventDetails.startDate).local().format("DD MMM YYYY");
            let eventStartTime = moment.utc(eventDetails.startTime).local().format("HH:mm");
            let eventEndTime = moment.utc(eventDetails.endTime).local().format("HH:mm");

            return `${eventDayMonthYear}, ${eventStartTime} - ${eventEndTime}`;
        }
        else {
            return "";
        }
    }

    /**
     * The event handler called when the event tab menu index gets changed
     * @param event The event details
     */
    onTabMenuIndexChange = (event: any) => {
        if (this.timeout) {
            window.clearTimeout(this.timeout);
        }

        this.setState({
            isLoading: true,
            searchText: "",
            activeTabIndex: this.state.isMobileView ? event?.index! : event?.activeIndex!
        });

        this.timeout = window.setTimeout(async () => {
            if (event) {
                this.getEvents();
            }
        }, 700);
    }

    /**
     * The event handler called when click on 'Close Registration' menu item of event table
     * @param eventId The event ID of which the registration need to be closed
     */
    onCloseRegistration = (eventId: string) => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("closeRegistration"),
            height: 746,
            width: 600,
            url: `${window.location.origin}/close-or-cancel-event?operationType=${EventOperationType.CloseRegistration}&eventId=${eventId}&teamId=${this.teamId}&isMobileView=${this.state.isMobileView}`
        }, (error: any, result: any) => {
            if (result) {
                this.setState((prevState: IManageEventsState) => (
                    {
                        notification: {
                            id: prevState.notification.id + 1,
                            message: this.localize("closeRegistrationSuccessfulMessage"),
                            type: ActivityStatus.Success
                        }
                    }));
            }
        });
    }

    onEditEvent = (eventId: string) => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("editEvent"),
            height: 746,
            width: 600,
            url: `${window.location.origin}/create-event?eventId=${eventId}&isDraft=${this.state.activeTabIndex === 0 ? true : false}`,
        }, (error: any, result: any) => {
            if (result) {
                if (result.isSuccess) {
                    this.setState((prevState: IManageEventsState) =>
                        ({
                            notification: {
                                id: prevState.notification.id + 1,
                                message: this.localize("updateEventSuccessfulMessage"),
                                type: ActivityStatus.Success
                            },
                            activeTabIndex: result.isCreateEvent ? ActiveEventsTabIndex : prevState.activeTabIndex
                        }), () => {
                            window.setTimeout(() => this.getEvents(), 2000);
                        });
                }
                else {
                    this.setState((prevState: IManageEventsState) =>
                        ({
                            notification: {
                                id: prevState.notification.id + 1,
                                message: this.localize("updateEventFailureMessage"),
                                type: ActivityStatus.Error
                            }
                        }));
                }
            }
        });
    }

    /**
     * The event handler called when sending reminder to the users of an event
     * @param eventId The event ID
     */
    onSendReminder = async (eventId: string) => {
        let response = await sendReminder(this.teamId, eventId);

        if (response.status === ResponseStatus.OK) {
            this.setState((prevState: IManageEventsState) =>
                ({
                    notification: {
                        id: prevState.notification.id + 1,
                        message: this.localize("sendReminderSuccessfulMessage"),
                        type: ActivityStatus.Success
                    }
                }));
        }
        else {
            this.setState((prevState: IManageEventsState) =>
                ({
                    notification: {
                        id: prevState.notification.id + 1,
                        message: this.localize("sendReminderFailureMessage"),
                        type: ActivityStatus.Error
                    }
                }));
        }
    }

    /**
     * The event handler called when click on 'Cancel Event' menu item of event table
     * @param eventId The event ID that need to be cancelled
     */
    onCancelEvent = (eventId: string) => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("cancelEvent"),
            height: 746,
            width: 600,
            url: `${window.location.origin}/close-or-cancel-event?operationType=${EventOperationType.CancelEvent}&eventId=${eventId}&teamId=${this.teamId}&isMobileView=${this.state.isMobileView}`
        }, (error: any, result: any) => {
            if (result) {
                let activeEvents = this.state.activeEvents ? [...this.state.activeEvents] : [];
                let updatedActiveEvents: Array<IEvent> = activeEvents.filter((event: IEvent) => event.eventId !== eventId);

                this.activeEventsCount -= 1;

                this.setState((prevState: IManageEventsState) => (
                    {
                        activeEvents: updatedActiveEvents,
                        notification: {
                            id: prevState.notification.id + 1,
                            message: this.localize("cancelEventSuccessfulMessage"),
                            type: ActivityStatus.Success,
                        }
                    }));
            }
        });
    }

    /**
     * The event handler called when deleting draft event
     * @param eventId The draft event ID that needs to be deleted
     */
    onDeleteDraftEvent = (eventId: string, eventName: string) => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("deleteEvent"),
            height: 200,
            width: 400,
            url: `${window.location.origin}/delete-draft?eventId=${eventId}&teamId=${this.teamId}`
        }, (error: any, result: any) => {
            if (result) {
                let draftEvents = this.state.draftEvents ? [...this.state.draftEvents] : [];
                let updatedDraftEvents: Array<IEvent> = draftEvents.filter((event: IEvent) => event.eventId !== eventId);

                this.draftEventsCount -= 1;

                this.setState((prevState: IManageEventsState) => ({
                    draftEvents: updatedDraftEvents,
                    notification: {
                        id: prevState.notification.id + 1,
                        message: this.localize("deleteDraftEvent"),
                        type: ActivityStatus.Success
                    }
                }));
            }
        });
    }

    onExportDetails = async (eventId: string, eventName: string) => {
        let response = await exportEventDetailsToCSV(this.teamId, eventId);

        if (response.status === ResponseStatus.OK) {
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const downloadLink = document.createElement('a');
            downloadLink.href = url;
            downloadLink.setAttribute('download', `${eventName}.csv`);
            downloadLink.click();

            this.setState((prevState: IManageEventsState) => ({
                notification: {
                    id: prevState.notification.id + 1,
                    message: this.localize("exportDetailsSuccessfulMessage"),
                    type: ActivityStatus.Success
                }
            }));
        }
        else {
            this.setState((prevState: IManageEventsState) => ({
                notification: {
                    id: prevState.notification.id + 1,
                    message: this.localize("exportDetailsFailureMessage"),
                    type: ActivityStatus.Error
                }
            }));
        }
    }

    /**
     * The event handler called after 'Enter' key press while searching events
     * @param event
     */
    onSearchEvents = (event: any) => {
        if (event.keyCode === 13 && this.state.searchText?.length && this.state.searchText.trim() !== "") {
            this.getEvents();
        }
    }

    /**
     * The event handler called when search text gets changed
     * @param event The input event object
     */
    onSearchTextChange = (event: any) => {
        if (!event.target.value.length) {
            this.setState({ searchText: "" }, () => {
                this.getEvents();
            });
        }
        else {
            this.setState({ searchText: event.target.value });
        }
    }

    /**
     * Formats events count to show on tab items
     * @param count The count of events
     */
    formatEventsCount = (count: number) => {
        if (count === -1) {
            return "";
        }
        else if (count < Constants.lazyLoadEventsCount) {
            return `(${count})`;
        }
        else {
            return `(${Constants.lazyLoadEventsCount}+)`;
        }
    }

    /** Gets tab menu items */
    getTabMenuItems = () => {
        return (
            [
                {
                    key: "draft-events",
                    content: `${this.localize("draftEvents")} ${this.formatEventsCount(this.draftEventsCount)}`
                },
                {
                    key: "active-events",
                    content: `${this.localize("activeEvents")} ${this.formatEventsCount(this.activeEventsCount)}`
                },
                {
                    key: "completed-events",
                    content: `${this.localize("completedEvents")} ${this.formatEventsCount(this.completedEventsCount)}`
                }
            ]
        );
    }

    onEventCreated = (err: any, result: any) => {
        if (result) {
            if (result.isSuccess) {
                this.setState((prevState: IManageEventsState) =>
                    ({
                        notification: {
                            id: prevState.notification.id + 1,
                            message: result.isDraft ? this.localize("createDraftSuccessfulMessage") : this.localize("createEventSuccessfulMessage"),
                            type: ActivityStatus.Success
                        },
                        activeTabIndex: result.isDraft ? DraftEventsTabIndex : ActiveEventsTabIndex
                    }), () => {
                        window.setTimeout(() => this.getEvents(), 2000);
                    });
            }
            else {
                this.setState((prevState: IManageEventsState) =>
                    ({
                        notification: {
                            id: prevState.notification.id + 1,
                            message: result.isDraft ? this.localize("createDraftFailureMessage") : this.localize("createEventFailureMessage"),
                            type: ActivityStatus.Error
                        }
                    }));
            }
        }

        this.props.microsoftTeams?.getContext((context: microsoftTeams.Context) => {
            this.teamId = context.teamId!;
        });
    }

    /** Open task module for manage categories */
    onManageCategoriesClick = () => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("manageCategories"),
            height: 600,
            width: 900,
            url: `${window.location.origin}/manage-categories`,
        }, () => { });
    }

    /** Open task module for create event */
    onCreateEventsClick = () => {
        this.props.microsoftTeams.tasks.startTask({
            title: this.localize("createEvent"),
            height: 746,
            width: 600,
            url: `${window.location.origin}/create-event`,
        }, this.onEventCreated);
    }

    /** Called when screen size gets updated; which sets the state to indicate whether mobile view enabled */
    onScreenSizeChange() {
        this.setState({ isMobileView: window.innerWidth <= Constants.maxWidthForMobileView });
    }

    /** The event handler called when mobile search box get closed */
    onMobileSearchBoxClose = () => {
        this.setState({ isMobileSearchBoxOpen: !this.state.isMobileSearchBoxOpen });

        /** If search box do not have the search text, then-
         - No need to call get events as it was already called when user explicitly makes search box empty
         - No need to call get events if user simply open and closes the search box
         */
        if (this.state.searchText?.trim().length) {
            this.setState({
                searchText: "",
            }, () => this.getEvents());
        }
    }

    /** Gets event table header */
    getEventsTableHeader = () => {
        return ({
            items: [
                {
                    content: this.localize("eventTitle")
                },
                {
                    content: this.localize("dateAndTime"),
                    design: { minWidth: "17vw", maxWidth: "17vw" }
                },
                {
                    content: this.localize("venue"),
                    design: { minWidth: "17vw", maxWidth: "17vw" }
                },
                {
                    content: this.localize("noOfRegistrations"),
                    design: { minWidth: "12vw", maxWidth: "12vw" }
                },
                {
                    content: this.localize("category"),
                    design: { minWidth: "15vw", maxWidth: "15vw" }
                },
                {
                    design: { minWidth: "5vw", maxWidth: "5vw" }
                }
            ]
        });
    }

    /**
     * Gets the event type text in grid based on event type
     * @param eventDetails The event details
     */
    getEventType = (eventDetails: IEvent) => {
        if (eventDetails.type === EventType.InPerson) {
            return eventDetails.venue;
        }
        else if (eventDetails.type === EventType.LiveEvent) {
            return this.localize("liveEvent");
        }
        else {
            return this.localize("teamsMeeting");
        }
    }

    /** Renders events' table */
    renderEvents() {
        let events: Array<IEvent> = [];

        if (this.state.activeTabIndex === DraftEventsTabIndex) {
            events = [...this.state.draftEvents];
        }
        else if (this.state.activeTabIndex === CompletedEventsTabIndex) {
            events = [...this.state.completedEvents];
        }
        else if (this.state.activeTabIndex === ActiveEventsTabIndex) {
            events = [...this.state.activeEvents];
        }

        if (events?.length > 0) {
            if (this.state.isMobileView) {
                let items: Array<any> = events.map((event: IEvent, index: number) => {
                    return {
                        key: `event-${index}`,
                        content:
                            <div className="manage-events-list-item-container">
                                <Flex space="between">
                                    <Flex column>
                                        <Text content={event.name} weight="bold" className="list-header" />
                                        <Flex className="list-elements" vAlign="center">
                                            <Text className="category-name" content={event.categoryName} weight="semibold" />
                                            <Text content="|" weight="semibold" className="divider" />
                                            <Text content={event.type === EventType.InPerson ? event.venue : this.localize("teamsMeeting")} weight="semibold" />
                                        </Flex>
                                        <Text className="list-elements" content={this.formatEventDateAndTime(event)} />
                                        <Text className="list-elements" content={`${this.localize("noOfRegistrations")}: ${event.registeredAttendeesCount}/${event.maximumNumberOfParticipants}`} />
                                    </Flex>
                                    <Flex.Item push>
                                        {
                                            event.status === EventStatus.Cancelled ? <Button disabled text iconOnly icon={<MoreIcon />} /> :
                                                <ManageEventsMenu
                                                    eventDetails={event}
                                                    onCancelEvent={this.onCancelEvent}
                                                    onCloseRegistration={this.onCloseRegistration}
                                                    onEditEvent={this.onEditEvent}
                                                    onExportDetails={this.onExportDetails}
                                                    onSendReminder={this.onSendReminder}
                                                    onDeleteDraftEvent={this.onDeleteDraftEvent}
                                                    dir={this.props.dir}
                                                />
                                        }
                                    </Flex.Item>
                                </Flex>
                            </div>
                    }
                });

                return (
                    <List className="manage-events-mobile-list-view" items={items} />
                );
            }
            else {
                let rows: Array<TableRowProps> = events.map((event: IEvent) => {
                    let eventVenue: string = this.getEventType(event);
                    let eventDateAndTime: string = this.formatEventDateAndTime(event);

                    return {
                        key: `row-${event.eventId}`,
                        items: [
                            {
                                content: <Text error={event.status === EventStatus.Cancelled ? true : false} content={event.name + (event.status === EventStatus.Cancelled ? ` (${this.localize("cancelledStatus")})` : "")} weight="bold" />,
                                truncateContent: true,
                                title: event.name
                            },
                            {
                                content: eventDateAndTime,
                                truncateContent: true,
                                title: eventDateAndTime,
                                design: { minWidth: "17vw", maxWidth: "17vw" }
                            },
                            {
                                content: eventVenue,
                                truncateContent: true,
                                title: eventVenue,
                                design: { minWidth: "17vw", maxWidth: "17vw" }
                            },
                            {
                                content: `${event.registeredAttendeesCount}/${event.maximumNumberOfParticipants}`,
                                design: { minWidth: "12vw", maxWidth: "12vw" }
                            },
                            {
                                content: event.categoryName,
                                truncateContent: true,
                                title: event.categoryName,
                                design: { minWidth: "15vw", maxWidth: "15vw" }
                            },
                            {
                                content: event.status === EventStatus.Cancelled ? <Button disabled text iconOnly icon={<MoreIcon />} /> :
                                    <ManageEventsMenu
                                        eventDetails={event}
                                        onCancelEvent={this.onCancelEvent}
                                        onCloseRegistration={this.onCloseRegistration}
                                        onEditEvent={this.onEditEvent}
                                        onExportDetails={this.onExportDetails}
                                        onSendReminder={this.onSendReminder}
                                        onDeleteDraftEvent={this.onDeleteDraftEvent}
                                        dir={this.props.dir}
                                    />,
                                design: { minWidth: "5vw", maxWidth: "5vw" }
                            }
                        ]
                    }
                });

                return (
                    <Table className="manage-events-table" header={this.getEventsTableHeader()} rows={rows} />
                );
            }
        }
        else {
            return (
                <NoContent message={this.localize("eventsNotAvailable")} />
            );
        }
    }

    /** Renders mobile view */
    renderMobileView = () => {
        return (
            <div className="container-manage-events">
                <Flex>
                    <Flex.Item push>
                        <Flex gap="gap.small">
                            <Button primary content={this.localize("manageCategories")} onClick={this.onManageCategoriesClick} />
                            <Button primary content={this.localize("createEvent")} onClick={this.onCreateEventsClick} />
                        </Flex>
                    </Flex.Item>
                </Flex>
                <Flex className="mobile-menu-manage-events" space="between" vAlign="center">
                    <SplitButton
                        button={this.getTabMenuItems()[this.state.activeTabIndex]}
                        menu={{ activeIndex: this.state.activeTabIndex, items: this.getTabMenuItems() }}
                        onMenuItemClick={(event: any, data: MenuItemProps | undefined) => this.onTabMenuIndexChange(data)}
                        style={{ "width": "40rem !important" }}
                        className="split-button"
                    />
                    <Flex.Item push>
                        <SearchIcon onClick={this.onMobileSearchBoxClose} />
                    </Flex.Item>
                </Flex>
                { this.state.isMobileSearchBoxOpen &&
                    <Flex className="manage-events-mobile-search-box" vAlign="center" gap="gap.small">
                        <Input
                            inverted
                            fluid
                            placeholder={this.localize("searchForEventsPlaceholder")}
                            input={{ design: { minWidth: "20rem", width: "100rem" } }}
                            onKeyUp={this.onSearchEvents}
                            onChange={this.onSearchTextChange}
                        />
                        <Flex.Item push>
                            <CloseIcon onClick={this.onMobileSearchBoxClose} />
                        </Flex.Item>
                    </Flex>
                }
                {
                    this.state.isLoading
                        ? <Loader className="loader" />
                        : <div key={this.state.infiniteScrollKey} className="manage-events-scroll-area-mobile">
                            <InfiniteScroll
                                pageStart={0}
                                loadMore={this.getEvents}
                                hasMore={this.state.loadMoreEvents}
                                initialLoad={false}
                                loader={<div><Loader /></div>}
                                useWindow={false}
                            >
                                {this.renderEvents()}
                            </InfiniteScroll>
                        </div>
                }
            </div>
        );
    }

    /** Renders desktop view */
    renderDesktopView = () => {
        return (
            <div className="container-manage-events">
                <Flex space="between" vAlign="center">
                    <TabMenu
                        tabItems={this.getTabMenuItems()}
                        defaultTabIndex={ActiveEventsTabIndex}
                        activeTabIndex={this.state.activeTabIndex!}
                        onTabIndexChange={this.onTabMenuIndexChange}
                    />
                    <Flex.Item push={this.props.dir === LanguageDirection.Ltr}>
                        <Flex gap="gap.medium" vAlign="center">
                            <Input
                                inverted
                                value={this.state.searchText}
                                icon={<SearchIcon />}
                                iconPosition={this.props.dir === LanguageDirection.Rtl ? "start" : "end"}
                                placeholder={this.localize("searchForEventsPlaceholder")}
                                input={{ design: { minWidth: "20rem", maxWidth: "20rem" } }}
                                onKeyUp={this.onSearchEvents}
                                onChange={this.onSearchTextChange}
                            />
                            <Flex gap="gap.small">
                                <Button content={this.localize("manageCategories")} onClick={this.onManageCategoriesClick} />
                                <Button
                                    className={this.props.dir === LanguageDirection.Rtl ? "rtl-right-margin-medium" : ""}
                                    icon={<AddIcon />}
                                    primary
                                    content={<Text className={this.props.dir === LanguageDirection.Rtl ? "rtl-right-margin-small" : ""} content={this.localize("createEvent")} />}
                                    onClick={this.onCreateEventsClick}
                                />
                            </Flex>
                        </Flex>
                    </Flex.Item>
                </Flex>
                {
                    this.state.isLoading
                        ? <Loader className="loader" />
                        : <div key={this.state.infiniteScrollKey} className="manage-events-scroll-area">
                            <InfiniteScroll
                                pageStart={0}
                                loadMore={this.getEvents}
                                hasMore={this.state.loadMoreEvents}
                                initialLoad={false}
                                loader={<div><Loader /></div>}
                                useWindow={false}
                            >
                                {this.renderEvents()}
                            </InfiniteScroll>
                        </div>
                }
            </div>
        );
    }

    /** Renders component */
    render() {
        return (
            <Fabric dir={this.props.dir}>
                <div className="container-div">
                    { this.state.isMobileView ? this.renderMobileView() : this.renderDesktopView()}
                    <ToastNotification dir={this.props.dir} notification={this.state.notification} />
                </div>
            </Fabric>
        );
    }
}

export default withTranslation()(withContext(ManageEvents));