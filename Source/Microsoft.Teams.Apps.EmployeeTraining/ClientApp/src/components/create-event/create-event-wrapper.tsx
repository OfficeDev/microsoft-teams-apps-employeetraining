// <copyright file="create-event-wrapper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import moment from "moment";
import { WithTranslation, withTranslation } from "react-i18next";
import { Flex, Provider, Loader, Text } from '@fluentui/react-northstar'
import { TFunction } from "i18next";
import CreateEventStep1 from "../create-event/create-event-step1";
import CreateEventStep2 from "../create-event/create-event-step2";
import CreateEventStep3 from "../create-event/create-event-step3";
import { IEvent } from "../../models/IEvent";
import { ISelectedUserGroup } from "../../models/ISelectedUserGroup";
import { ISelectedDropdownItem } from "../../models/ISelectedDropdownItem";
import { EventType } from "../../models/event-type";
import { EventAudience } from "../../models/event-audience";
import { IConstantDropdownItem } from "../../constants/resources";
import { getEventAsync } from "../../api/common-api";
import { ResponseStatus } from "../../constants/constants";
import { EventStatus } from "../../models/event-status";
import EventDetails from "../common/event-details/event-details";
import withContext, { IWithContext } from "../../providers/context-provider";
import { getEventCategoriesAsync } from "../../api/create-event-api";
import { Fabric } from "@fluentui/react";

import "./create-event.css";

interface ICreateEventProps extends WithTranslation, IWithContext {
}

export interface ICreateEventState {
    currentEventStep: number,
    eventDetails: IEvent,
    selectedCategory: ISelectedDropdownItem | undefined,
    selectedEvent: ISelectedDropdownItem | undefined,
    selectedAudience: ISelectedDropdownItem | undefined,
    selectedUserGroups: Array<ISelectedUserGroup>,
    categories: Array<IConstantDropdownItem>,
    isEdit: boolean,
    isDraft: boolean,
    isLoading: boolean,
    displayReadonly: boolean
}

class CreateEventWrapper extends React.Component<ICreateEventProps, ICreateEventState> {
    readonly localize: TFunction;
    minDate = new Date();
    params: { eventId?: string | undefined, isDraft?: boolean | undefined } = { eventId: undefined, isDraft: false };
    teamId: string;

    /** Constructor which initializes state */
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.minDate.setDate(this.minDate.getDate() + 1);
        this.teamId = "";
        let search = window.location.search;
        let params = new URLSearchParams(search);
        this.params.eventId = params.get("eventId")!;
        this.params.isDraft = params.get("isDraft")! === "true" ? true : false;

        this.state = {
            currentEventStep: 1,
            categories: new Array<IConstantDropdownItem>(),
            displayReadonly: false,
            eventDetails: {
                categoryId: "",
                categoryName: "",
                createdBy: "",
                createdOn: new Date(),
                description: "",
                endDate: this.minDate,
                eventId: "",
                graphEventId: "",
                isAutoRegister: false,
                isRegistrationClosed: false,
                maximumNumberOfParticipants: 0,
                meetingLink: "",
                name: "",
                numberOfOccurrences: 1,
                photo: "",
                registeredAttendeesCount: 0,
                startDate: this.minDate,
                status: 0,
                teamId: "",
                type: EventType.Teams,
                venue: "",
                audience: EventAudience.Public,
                endTime: undefined,
                startTime: undefined,
                mandatoryAttendees: "",
                optionalAttendees: "",
                registeredAttendees: "",
                selectedUserOrGroupListJSON: "",
                autoRegisteredAttendees: "",
            },
            selectedCategory: undefined,
            selectedEvent: undefined,
            selectedAudience: undefined,
            selectedUserGroups: new Array<ISelectedUserGroup>(),
            isEdit: false,
            isDraft: false,
            isLoading: true
        };
    }

    componentDidMount() {
        this.getAllCategories();
    }

    /** Gets teams context from HOC as props */
    componentWillReceiveProps(nextProps: ICreateEventProps) {
        if (nextProps.teamsContext && nextProps.teamsContext !== this.props.teamsContext) {
            this.teamId = nextProps.teamsContext.teamId!;

            if (this.params.eventId) {
                this.getEventDetailsToUpdate();
            }
        }
    }

    /* Updating Event Details required */
    getEventDetailsToUpdate = async () => {
        var response = await getEventAsync(this.params.eventId!, this.teamId);
        if (response.status === ResponseStatus.OK) {
            let eventDetails = response.data as IEvent;
            let selectedUsersOrGroups = Array<ISelectedUserGroup>();

            if (eventDetails.selectedUserOrGroupListJSON) {
                selectedUsersOrGroups = JSON.parse(eventDetails.selectedUserOrGroupListJSON);
            }
            eventDetails.startDate = moment.utc(eventDetails.startDate).local().toDate();
            eventDetails.endDate = moment.utc(eventDetails.endDate).local().toDate();
            eventDetails.startTime = moment.utc(eventDetails.startTime).local().toDate();
            eventDetails.endTime = moment.utc(eventDetails.endTime).local().toDate();

            if (eventDetails.status === EventStatus.Cancelled || eventDetails.endDate < new Date()) {
                this.setState({ isLoading: false, displayReadonly: true, eventDetails: eventDetails }, this.getAllCategories);
            } else {
                this.setState({ isLoading: false, eventDetails: eventDetails, selectedUserGroups: selectedUsersOrGroups, isEdit: true, isDraft: this.params.isDraft! }, this.getAllCategories);
            }

        }
        else {
            this.setState({
                isLoading: false
            });
        }
    }

    /** Event Handler for fetching the category list and rendering it */
    getAllCategories = async () => {
        let response = await getEventCategoriesAsync();

        if (response.status === ResponseStatus.OK) {
            let categories: any = response.data.map((category: any) => {
                return { name: category.name, id: category.categoryId };
            });

            let selectedCategory: ISelectedDropdownItem;
            if (this.state.eventDetails.categoryId) {
                let category = categories.find((category: any) => category.id === this.state.eventDetails.categoryId);
                selectedCategory = { header: category.name, key: category.id };

                this.setState({ categories: categories, selectedCategory: selectedCategory, isLoading: false });
            }
            else {
                this.setState({ categories: categories, isLoading: false });
            }
        }
        else {
            this.setState({ isLoading: false });
        }
    }

    /**
    * Set state of an event on navigating to next step
    * @param currentStep shows the event step which is currently active
    * @param stepEventState current state of the event
    */
    setEventStep = (currentStep: number, stepEventState: ICreateEventState) => {
        this.setState({ currentEventStep: currentStep, eventDetails: stepEventState.eventDetails, selectedAudience: stepEventState.selectedAudience, selectedCategory: stepEventState.selectedCategory, selectedEvent: stepEventState.selectedEvent, selectedUserGroups: stepEventState.selectedUserGroups, categories: stepEventState.categories });
    }

    /** Renders component based on Add, Edit or Delete */
    renderOperation = () => {
        switch (this.state.currentEventStep) {
            case 1:
                return <CreateEventStep1 eventPageState={{ ...this.state }} navigateToPage={this.setEventStep} />
            case 2:
                return <CreateEventStep2 eventPageState={{ ...this.state }} navigateToPage={this.setEventStep} dir={this.props.dir}/>
            case 3:
                return <CreateEventStep3 eventPageState={{ ...this.state }} navigateToPage={this.setEventStep} />
            default:
                return <CreateEventStep1 eventPageState={{ ...this.state }} navigateToPage={this.setEventStep} />
        }
    }

    /** Renders the component */
    render() {
        if (!this.state.isLoading && !this.state.categories?.length) {
            return (
                <Provider>
                    <Flex>
                        <div className="task-module-container event-task-module">
                            <Text error content={this.localize("categoriesNotAvailableError")} weight="semibold" />
                        </div>
                    </Flex>
                </Provider>
            );
        }

        if (this.state.displayReadonly) {
            return <EventDetails eventDetails={this.state.eventDetails} />
        }
        else {
            return (
                <Fabric dir={this.props.dir}>
                    <Provider>
                        <Flex>
                            <div className="task-module-container">
                                {!this.state.isLoading && !this.state.displayReadonly && this.renderOperation()}
                                {this.state.isLoading && <Loader className="loader" />}
                            </div>
                        </Flex>
                    </Provider>
                </Fabric>
            );
        }

    }
}

export default withTranslation()(withContext(CreateEventWrapper));