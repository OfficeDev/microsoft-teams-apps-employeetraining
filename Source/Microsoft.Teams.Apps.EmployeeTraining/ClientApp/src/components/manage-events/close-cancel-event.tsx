// <copyright file="close-cancel-event.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import moment from "moment";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IEvent } from "../../models/IEvent";
import { ResponseStatus } from "../../constants/constants";
import { EventOperationType } from "../../models/event-operation-type";
import { EventStatus } from "../../models/event-status";
import { getEventAsync } from "../../api/manage-events-api";
import { getUserProfiles } from "../../api/user-group-api"
import { closeEventRegistrationsAsync, cancelEventAsync } from "../../api/manage-events-api";
import EventDetails from "../event-operation-task-module/event-details";
import withContext, { IWithContext } from "../../providers/context-provider";

interface ICloseCancelEventState {
    isLoading: boolean,
    isOperationInProgress: boolean,
    eventDetails: IEvent | undefined,
    eventCreatedBy: string,
    isErrorGettingEventDetails: boolean,
    isFailedToCloseOrCancelEvent: boolean,
    eventOperationType: EventOperationType
}

interface ICloseCancelEventProps extends IWithContext, WithTranslation {
}

class CloseCancelEvent extends React.Component<ICloseCancelEventProps, ICloseCancelEventState> {
    readonly localize: TFunction;
    isMobileView: boolean;

    constructor(props) {
        super(props);

        this.localize = this.props.t;
        this.isMobileView = false;

        this.state = {
            isLoading: true,
            isOperationInProgress: false,
            eventDetails: undefined,
            eventCreatedBy: "",
            isErrorGettingEventDetails: false,
            isFailedToCloseOrCancelEvent: false,
            eventOperationType: EventOperationType.None
        }
    }

    componentDidMount() {
        this.getEventDetailsAsync();
    }

    /** Gets event details */
    getEventDetailsAsync = async () => {
        let queryParam = new URLSearchParams(window.location.search);
        let operationType = queryParam.get("operationType") ?? "0";
        let eventId = queryParam.get("eventId") ?? "0";
        let teamId = queryParam.get("teamId") ?? "0";
        this.isMobileView = queryParam.get("isMobileView") === "true" ? true : false;

        let response = await getEventAsync(eventId!, teamId!);

        if (response.status === ResponseStatus.OK && response.data) {
            let eventDetails: IEvent = response.data;
            let eventOperationType: EventOperationType = EventOperationType.None;

            if (eventDetails.status === EventStatus.Active && new Date() < moment.utc(eventDetails.endDate).local().toDate()) {
                if (operationType === EventOperationType.CloseRegistration.toString()) {
                    if (!eventDetails.isRegistrationClosed) {
                        eventOperationType = EventOperationType.CloseRegistration;
                    }
                }
                else if (operationType === EventOperationType.CancelEvent.toString()) {
                    eventOperationType = EventOperationType.CancelEvent;
                }
            }

            this.setState({isLoading: false, eventDetails, eventOperationType }, () => {
                if (this.state.eventDetails) {
                    this.getUserProfileAsync(this.state.eventDetails.createdBy);
                }
            });
        }
        else {
            this.setState({ isLoading: false, isErrorGettingEventDetails: true });
        }
    }

    /**
     * Get event creator information
     * @param userId The user ID of which information to get
     */
    getUserProfileAsync = async (userId: string) => {
        let user: Array<string> = [userId];
        let response = await getUserProfiles(user);

        if (response.status === ResponseStatus.OK && response.data) {
            let userInfo = response.data[0];
            this.setState({ eventCreatedBy: userInfo.displayName });
        }
    }

    /** Called when click on 'Close Registration' or 'Cancel' event */
    onCloseOrCancelEvent = async () => {
        this.setState({ isFailedToCloseOrCancelEvent: false, isOperationInProgress: true });

        let response;
        let teamId = this.state.eventDetails ? this.state.eventDetails.teamId : "0";
        let eventId = this.state.eventDetails ? this.state.eventDetails.eventId : "0";


        switch (this.state.eventOperationType) {
            case EventOperationType.CloseRegistration:
                response = await closeEventRegistrationsAsync(teamId, eventId);
                break;

            case EventOperationType.CancelEvent:
                response = await cancelEventAsync(teamId, eventId);
                break;

            default:
                break;
        }

        if (response && response.status === ResponseStatus.OK && response.data === true) {
            this.props.microsoftTeams.tasks.submitTask({ isSuccess: true });
        }
        else {
            this.setState({ isFailedToCloseOrCancelEvent: true, isOperationInProgress: false });
        }
    }

    /** Renders component */
    render() {
        return (
            <EventDetails
                dir={this.props.dir}
                eventDetails={this.state.eventDetails}
                eventCreatedByName={this.state.eventCreatedBy}
                eventOperationType={this.state.eventOperationType}
                isLoadingEventDetails={this.state.isLoading}
                isFailedToGetEventDetails={this.state.isErrorGettingEventDetails}
                isOperationInProgress={this.state.isOperationInProgress}
                isOperationFailed={this.state.isFailedToCloseOrCancelEvent}
                isMobileView={this.isMobileView}
                onPerformOperation={this.onCloseOrCancelEvent}
            />
        );
    }
}

export default withTranslation()(withContext(CloseCancelEvent));