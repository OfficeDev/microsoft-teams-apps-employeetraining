// <copyright file="discover-events.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Button, Flex, Input, Loader, MenuButton, MenuProps, Provider, SearchIcon, CloseIcon } from "@fluentui/react-northstar";
import { Fabric } from "@fluentui/react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IEvent } from "../../models/IEvent";
import { EventSearchType } from "../../models/event-search-type";
import Constants, { ResponseStatus } from "../../constants/constants";
import { SortBy } from "../../models/sort-by";
import { ICategory } from "../../models/ICategory";
import UserEvents from "../user-events-wrapper/user-events-wrapper";
import NoContent from "../no-content-page/no-content-page"
import FilterBar from "../filter-bar/filter-bar";
import TabMenu from "../tab-menu/tab-menu"
import { getEventsAsync } from "../../api/user-events-api";
import { Icon } from '@fluentui/react';
import { ITeamsChannelMember } from "../../models/ITeamsChannelMember";
import ToastNotification from "../toast-notification/toast-notification";
import { ActivityStatus } from "../../models/activity-status";
import { IToastNotification } from "../../models/IToastNotification";
import { getEventCategoriesAsync } from "../../api/create-event-api";
import { getAllLnDTeamMembersAsync } from "../../api/LnD-team-api";
import { EventOperationType } from "../../models/event-operation-type";
import Resources from "../../constants/resources";
import { clearMobileFilterLocalStorage } from "../../helpers/mobile-filter-helper";
import withContext, { IWithContext } from "../../providers/context-provider";
import { LanguageDirection } from "../../models/language-direction";

import "./discover-events.css";

interface IDiscoverEventsState {
    mandatoryEvents: Array<IEvent>,
    allEvents: Array<IEvent>,
    hasMoreEvents: boolean,
    userEventsContainerKey: number,
    isLoadingEvents: boolean,
    activeTabIndex: number | string | undefined,
    isFilterOpen: boolean,
    searchText: string,
    filteredCategories: string,
    filteredUsers: string,
    sortByFilter: number,
    categoriesInFilter: Array<ICategory>,
    lnDTeamMembersInFilter: Array<ITeamsChannelMember>,
    isResetFilter: boolean,
    notification: IToastNotification,
    isMobileView: boolean,
    isMobileSearchBoxOpen: boolean
}

interface IDiscoverEventsProps extends WithTranslation, IWithContext {
}

/** The tab index for 'Mandatory events' tab */
const MandatoryEventsTabIndex: number = 0;

/** The tab index for 'All events' */
const AllEventsTabIndex: number = 1;

/** Renders all events for user created by LnD team */
class DiscoverEvents extends React.Component<IDiscoverEventsProps, IDiscoverEventsState> {
    localize: TFunction;
    searchText: string;
    timeout: number | null;
    mandatoryEventsCount: number;
    allEventsCount: number;

    constructor(props: IDiscoverEventsProps) {
        super(props);

        this.searchText = "";
        this.localize = this.props.t;
        this.timeout = null;
        this.mandatoryEventsCount = -1;
        this.allEventsCount = -1;

        this.state = {
            mandatoryEvents: [],
            allEvents: [],
            hasMoreEvents: false,
            userEventsContainerKey: 0,
            isLoadingEvents: true,
            activeTabIndex: MandatoryEventsTabIndex,
            isFilterOpen: false,
            searchText: "",
            filteredCategories: "",
            filteredUsers: "",
            sortByFilter: SortBy.Recent,
            categoriesInFilter: [],
            lnDTeamMembersInFilter: [],
            isResetFilter: false,
            notification: { id: 0, message: "", type: ActivityStatus.None },
            isMobileView: false,
            isMobileSearchBoxOpen: false
        }
    }

    componentDidMount() {
        window.addEventListener("resize", this.screenResize.bind(this));
        this.screenResize();

        this.loadEventCategories();
        this.loadAllLnDTeamMembers();

        clearMobileFilterLocalStorage();

        let mandatoryEvents = this.getEventsAsync(EventSearchType.MandatoryEventsForUser);
        let allEvents = this.getEventsAsync(EventSearchType.AllPublicPrivateEventsForUser);

        Promise.all([mandatoryEvents, allEvents])
            .then((results) => {
                this.setTotalEventsCount(EventSearchType.MandatoryEventsForUser, results[0].length);
                this.setTotalEventsCount(EventSearchType.AllPublicPrivateEventsForUser, results[1].length);
            })
            .finally(() => {
                this.getEvents();
            });
    }

    screenResize = () => {
        this.setState({ isMobileView: window.innerWidth <= Constants.maxWidthForMobileView });
    }

    /** Get all event categories and load in filter */
    loadEventCategories = async () => {
        let response = await getEventCategoriesAsync();

        if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
            this.setState({ categoriesInFilter: response.data });
        }
    }

    /** Get all LnD teams' members and load in filter */
    loadAllLnDTeamMembers = async () => {
        let response = await getAllLnDTeamMembersAsync();

        if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
            this.setState({ lnDTeamMembersInFilter: response.data });
        }
    }

    /**
     * Gets all events for specified page number
     * @param eventSearchType The type of user events to get
     * @param pageNumber The page number of which events to get
     */
    getEventsAsync = async (eventSearchType: EventSearchType, pageNumber: number = 0) => {
        let response: any;

        if (eventSearchType === EventSearchType.MandatoryEventsForUser) {
            response = await getEventsAsync(
                this.state.searchText,
                pageNumber,
                EventSearchType.MandatoryEventsForUser,
                this.state.filteredUsers,
                this.state.filteredCategories,
                this.state.sortByFilter);
        }
        else {
            response = await getEventsAsync(this.state.searchText,
                pageNumber,
                EventSearchType.AllPublicPrivateEventsForUser,
                this.state.filteredUsers,
                this.state.filteredCategories,
                this.state.sortByFilter);
        }

        let result: Array<IEvent> = [];

        if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
            result = response.data;
        }

        return result;
    }

    /**
     * Gets all events for specified page number
     * @param eventSearchType The type of user events to get
     * @param pageNumber The page number of which events to get
     */
    loadEvents = async (eventSearchType: EventSearchType, pageNumber: number = 0) => {
        let events: Array<IEvent> = await this.getEventsAsync(eventSearchType, pageNumber);

        if (events) {
            switch (eventSearchType) {
                case EventSearchType.MandatoryEventsForUser:
                    if (pageNumber > 0) {
                        let response = [...events];
                        let mandatoryEvents = [...this.state.mandatoryEvents];

                        let updatedEventResponse = response.filter((event: IEvent) =>
                            mandatoryEvents.findIndex((eventDetails: IEvent) => eventDetails.eventId === event.eventId) === -1
                        );

                        this.setState({
                            mandatoryEvents: mandatoryEvents.concat(updatedEventResponse),
                            hasMoreEvents: events.length < Constants.lazyLoadEventsCount ? false : true
                        });
                    }
                    else {
                        this.setTotalEventsCount(EventSearchType.MandatoryEventsForUser, events.length);

                        this.setState({
                            mandatoryEvents: [...events],
                            hasMoreEvents: events.length < Constants.lazyLoadEventsCount ? false : true
                        });
                    }

                    break;

                case EventSearchType.AllPublicPrivateEventsForUser:
                    if (pageNumber > 0) {
                        let response = [...events];
                        let allEvents = [...this.state.allEvents];

                        let updatedEventResponse = response.filter((event: IEvent) =>
                            allEvents.findIndex((eventDetails: IEvent) => eventDetails.eventId === event.eventId) === -1);

                        this.setState({
                            allEvents: allEvents.concat(updatedEventResponse),
                            hasMoreEvents: events.length < Constants.lazyLoadEventsCount ? false : true
                        });
                    }
                    else {
                        this.setTotalEventsCount(EventSearchType.AllPublicPrivateEventsForUser, events.length);

                        this.setState({
                            allEvents: [...events],
                            hasMoreEvents: events.length < Constants.lazyLoadEventsCount ? false : true
                        });
                    }
                    break;

                default:
                    break;
            }
        }
        else {
            if (pageNumber === 0) {
                this.setTotalEventsCount(eventSearchType, 0);

                if (eventSearchType === EventSearchType.MandatoryEventsForUser) {
                    this.setState({ mandatoryEvents: [], hasMoreEvents: false });
                }
                else {
                    this.setState({ allEvents: [], hasMoreEvents: false });
                }
            }
            else {
                this.setState({ hasMoreEvents: false });
            }
        }
    }

    /**
     * Sets the total event count for selected tab
     * @param count The total count to set
     */
    setTotalEventsCount = (eventSeatchType: EventSearchType, count: number) => {
        if ((!this.state.searchText || this.state.searchText.trim() === "") && !this.state.filteredCategories?.length && !this.state.filteredUsers?.length) {
            switch (eventSeatchType) {
                case EventSearchType.MandatoryEventsForUser:
                    this.mandatoryEventsCount = count;
                    break;

                case EventSearchType.AllPublicPrivateEventsForUser:
                    this.allEventsCount = count;
                    break;

                default:
                    break;
            }
        }
    }

    /**
     * The event handler called when click on event
     * @param eventDetails The event details
     */
    onEventClick = (eventDetails: IEvent) => {
        this.props.microsoftTeams.tasks.startTask({
            url: `${window.location.origin}/register-remove?eventId=${eventDetails.eventId}&teamId=${eventDetails.teamId}&isMobileView=${this.state.isMobileView}`,
            height: 746,
            width: 600,
            title: this.localize("eventDetailsStep1"),
        }, (error:any, result:any) => {
                if (result) {
                    let events: Array<IEvent> = this.state.activeTabIndex === MandatoryEventsTabIndex ? [...this.state.mandatoryEvents] : [...this.state.allEvents];
                    let eventAtIndex: number = events?.findIndex((event: IEvent) => event.eventId === eventDetails.eventId);

                    let eventToUpdate: IEvent = events[eventAtIndex];
                    eventToUpdate.registeredAttendeesCount = result.type === EventOperationType.Register ? eventToUpdate.registeredAttendeesCount + 1 : eventToUpdate.registeredAttendeesCount - 1;

                    if (this.state.activeTabIndex === MandatoryEventsTabIndex) {
                        this.setState((prevState: IDiscoverEventsState) => (
                            {
                                mandatoryEvents: events,
                                notification: {
                                    id: prevState.notification.id + 1,
                                    message: result.type === EventOperationType.Register ? this.localize("eventRegistrationSuccessfulMessage") : this.localize("eventUnregistrationSuccessfulMessage"),
                                    type: ActivityStatus.Success
                                }
                            }
                        ));
                    }
                    else {
                        this.setState((prevState: IDiscoverEventsState) => (
                            {
                                allEvents: events,
                                notification: {
                                    id: prevState.notification.id + 1,
                                    message: result.type === EventOperationType.Register ? this.localize("eventRegistrationSuccessfulMessage") : this.localize("eventUnregistrationSuccessfulMessage"),
                                    type: ActivityStatus.Success
                                }
                            }
                        ));
                    }
                }
        });
    }

    /** Get called when tab selection change */
    onTabIndexChange = (event: MenuProps | undefined) => {
        if (this.timeout) {
            window.clearTimeout(this.timeout);
        }

        clearMobileFilterLocalStorage();

        this.setState({ isLoadingEvents: true, activeTabIndex: event?.activeIndex!, searchText: "", filteredCategories: "", filteredUsers: "", sortByFilter: SortBy.Recent, isResetFilter: true }, () => {
            this.timeout = window.setTimeout(async () => {
                if (event) {
                    this.getEvents();
                }
            }, 700);
        });
    }

    /** Get called when 'Enter' key pressed in 'Search events' textbox */
    onSearchEvents = (event: any) => {
        if (event.keyCode === 13 && this.state.searchText.trim() !== "") {
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

    /** The event handler to toggle the state of filter bar */
    onFilterBarToggle = () => {
        this.setState((prevState) => ({
            isFilterOpen: !prevState.isFilterOpen,
            isResetFilter: false
        }));
    }

    /** The event handler called when click on filter icon and mobile mode is ON */
    onMobileFilterButtonClick = () => {
        this.props.microsoftTeams.tasks.startTask({
            url: `${window.location.origin}/mobile-filter`,
            height: 746,
            width: 600,
            title: this.localize("mobileFilterTitle"),
        }, (error: any, result: any) => {
                if (result && result.isFilterStateChanged) {
                    let filteredCategories = localStorage.getItem(Resources.userEventsMobileFilteredCategoriesLocalStorageKey);
                    let filteredUsers = localStorage.getItem(Resources.userEventsMobileFilteredUsersLocalStorageKey);
                    let sortBy = localStorage.getItem(Resources.userEventsMobileSortByFilterLocalStorageKey);

                    this.setState({
                        filteredCategories: filteredCategories ? filteredCategories : "",
                        filteredUsers: filteredUsers ? filteredUsers : "",
                        sortByFilter: sortBy ? sortBy as unknown as number : 0
                    }, () => this.getEvents());
                }
        });
    }

    /** Event handler called when click on close filter bar which closes it and clears all filters those were applied */
    onFilterBarClose = (isFilterStateChanged: boolean) => {
        this.setState((prevState) => ({
            isFilterOpen: !prevState.isFilterOpen,
            filteredCategories: "",
            filteredUsers: "",
            sortByFilter: SortBy.Recent,
            isResetFilter: true
        }), () => {
                if (isFilterStateChanged) {
                    this.getEvents();
                }
        });
    }

    /**
     * The event handler called when to get events based on filtered values
     * @param selectedCategories The selected categories in filter bar
     * @param selectedUsers The selected users in filter bar
     * @param sortBy The selected sort by value in filter bar
     */
    onFilterChange = (selectedCategories: Array<string>, selectedUsers: Array<string>, sortBy: number) => {
        let filteredCategories: Array<string> = selectedCategories ? selectedCategories : [];
        let filteredUsers: Array<string> = selectedUsers ? selectedUsers : [];

        this.setState({
            filteredCategories: filteredCategories.join(";"),
            filteredUsers: filteredUsers.join(";"),
            sortByFilter: sortBy
        }, () => {
            this.getEvents();
        });
    }

    /** The event handler called when mobile search box get closed */
    onMobileSearchBoxClose = () => {
        this.setState({ isMobileSearchBoxOpen: !this.state.isMobileSearchBoxOpen });

        /** If search box do not have the search text, then-
         - No need to call get events as it was already called when user explicitly makes search box empty
         - No need to call get events if user simply open and closes the search box
         */
        if (this.state.searchText?.trim().length) {
            this.setState({ searchText: "" }, () => this.getEvents());
        }
    }

    /**
     * Get events based on current tab selection
     * @param pageNumber The page number of which events to get
     */
    getEvents = (pageNumber: number = 0) => {
        if (pageNumber === 0) {
            this.setState((prevState: IDiscoverEventsState) =>
                ({ isLoadingEvents: true, hasMoreEvents: false, userEventsContainerKey: prevState.userEventsContainerKey + 1, isResetFilter: false }));
        }

        switch (this.state.activeTabIndex) {
            case MandatoryEventsTabIndex:
                this.loadEvents(EventSearchType.MandatoryEventsForUser, pageNumber)
                    .finally(() => {
                        this.setState({ isLoadingEvents: false });
                    });
                break;

            case AllEventsTabIndex:
                this.loadEvents(EventSearchType.AllPublicPrivateEventsForUser, pageNumber)
                    .finally(() => {
                        this.setState({ isLoadingEvents: false });
                    });
                break;

            default:
                this.setState({ isLoadingEvents: false });
                break;
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
    renderTabMenuItems = () => {
        return (
            [
                {
                    key: "mandatory-user-events",
                    content: `${this.localize("mandatoryEventsTab")} ${this.formatEventsCount(this.mandatoryEventsCount)}`
                },
                {
                    key: "all-user-events",
                    content: `${this.localize("allEventsTab")} ${this.formatEventsCount(this.allEventsCount)}`
                },
            ]
        );
    }

    /** Renders user events */
    renderEvents = () => {
        if (this.state.isLoadingEvents) {
            return <Provider><Loader className="loader" /></Provider>;
        }

        let events: Array<IEvent> = [];

        if (this.state.activeTabIndex === MandatoryEventsTabIndex) {
            events = [...this.state.mandatoryEvents];
        }
        else if (this.state.activeTabIndex === AllEventsTabIndex) {
            events = [...this.state.allEvents];
        }

        if (!events || events.length === 0) {
            return (
                <NoContent message={this.localize("eventsNotAvailable")} />
            );
        }

        return (
            <Flex className={this.state.isFilterOpen ? "filter-open" : undefined}>
                <UserEvents
                    key={`discover-events-${this.state.userEventsContainerKey}`}
                    events={events}
                    hasMoreEvents={this.state.hasMoreEvents}
                    loadMoreEvents={this.getEvents}
                    onClick={this.onEventClick}
                />
            </Flex>
        );
    }

    renderFilterButtonIcon = () => {
        if ((this.state.filteredCategories && this.state.filteredCategories.length > 0)
            || (this.state.filteredUsers && this.state.filteredUsers.length > 0)) {
            return <Icon iconName="FilterSolid" />
        }
        else {
            return <Icon iconName="Filter" />
        }
    }

    /** Renders the desktop view */
    renderDesktopView = () => {
        return (
            <Fabric dir={this.props.dir}>
                <Flex column>
                    <Flex space="between" vAlign="center">
                        <TabMenu defaultTabIndex={MandatoryEventsTabIndex} activeTabIndex={this.state.activeTabIndex!} tabItems={this.renderTabMenuItems()} onTabIndexChange={this.onTabIndexChange} />
                        <Flex.Item push={this.props.dir === LanguageDirection.Ltr}>
                            <Flex gap="gap.medium" vAlign="center">
                                <MenuButton
                                    className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-medium" : ""}
                                    trigger={
                                        <Button icon={this.renderFilterButtonIcon()}
                                            content={this.localize("filterButtonText")}
                                            onClick={this.onFilterBarToggle}
                                            data-testid="filterbutton"
                                        />}
                            />
                                <Input
                                    inverted
                                    value={this.state.searchText}
                                    icon={<SearchIcon />}
                                    iconPosition={this.props.dir === LanguageDirection.Rtl ? "start" : "end"}
                                    placeholder={this.localize("searchForEventsPlaceholder")}
                                    input={{ design: { minWidth: "20rem", maxWidth: "20rem" } }}
                                    onKeyUp={this.onSearchEvents}
                                    onChange={this.onSearchTextChange}
                                    data-testid="search_input"
                                />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    <FilterBar
                        isVisible={this.state.isFilterOpen}
                        isReset={this.state.isResetFilter}
                        categoryList={this.state.categoriesInFilter}
                        createdByList={this.state.lnDTeamMembersInFilter}
                        onFilterBarCloseClick={this.onFilterBarClose}
                        onFilterChange={this.onFilterChange}
                    />
                </Flex>
                {this.renderEvents()}
            </Fabric>
        );
    }

    /** Renders the mobile view */
    renderMobileView = () => {
        return (
            <Fabric dir={this.props.dir}>
                <Flex column>
                    <Flex space="between">
                        <TabMenu defaultTabIndex={MandatoryEventsTabIndex} activeTabIndex={this.state.activeTabIndex!} tabItems={this.renderTabMenuItems()} onTabIndexChange={this.onTabIndexChange} />
                        <Flex.Item push>
                            <Flex gap="gap.medium" vAlign="center">
                                <MenuButton
                                    trigger={
                                        <Button
                                            text
                                            iconOnly
                                            icon={this.renderFilterButtonIcon()}
                                            onClick={this.onMobileFilterButtonClick}
                                        />}
                                />
                                <SearchIcon onClick={() => this.setState({ isMobileSearchBoxOpen: !this.state.isMobileSearchBoxOpen })} />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    {this.state.isMobileSearchBoxOpen && !this.state.isFilterOpen &&
                        <Flex className="search-box" vAlign="center" gap="gap.small">
                            <Input
                                inverted
                                fluid
                                value={this.state.searchText}
                                placeholder={this.localize("searchForEventsPlaceholder")}
                                input={{ design: { minWidth: "20rem", width: "100rem" } }}
                                onKeyUp={this.onSearchEvents}
                                onChange={this.onSearchTextChange}
                            />
                            <Flex.Item push>
                                <CloseIcon className="close-searchbox-icon" onClick={this.onMobileSearchBoxClose} />
                            </Flex.Item>
                        </Flex>
                    }
                </Flex>
                {!this.state.isFilterOpen && this.renderEvents()}
            </Fabric>
        );
    }

    /** Renders component */
    render() {
        return (
            <div className="container-div">
                <div className="discover-events-container">
                    {this.state.isMobileView ? this.renderMobileView() : this.renderDesktopView()}
                    <ToastNotification dir={this.props.dir} notification={this.state.notification} />
                </div>
            </div>
        );
    }
}

export default withTranslation()(withContext(DiscoverEvents));