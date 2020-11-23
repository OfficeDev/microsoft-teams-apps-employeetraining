// <copyright file="event-details.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Label, Image, EyeSlashIcon, Divider, EyeIcon, Avatar, Layout, Loader } from "@fluentui/react-northstar";
import { Icon } from 'office-ui-fabric-react';
import { IEvent } from "../../../models/IEvent";
import { useTranslation } from 'react-i18next';
import { EventAudience } from "../../../models/event-audience";
import { EventType } from "../../../models/event-type";
import { getUserProfiles } from "../../../api/user-group-api"
import { ResponseStatus } from "../../../constants/constants";
import moment from "moment";

import "../../manage-events/manage-events.css";

interface IEventDetailsProps {
    eventDetails: IEvent
}

/** Renders component to close event registrations or cancel an event */
const EventDetails: React.FunctionComponent<IEventDetailsProps> = props => {
    const localize = useTranslation().t;

    let [eventDetails, setEventDetails] = React.useState(props.eventDetails);
    let [isLoading, setLoading] = React.useState(false);
    let [errorGettingEventDetails, setEventDetailsError] = React.useState(false);
    let [createdByName, setCreatedByName] = React.useState("");

    React.useEffect(() => {
        getUserProfile(props.eventDetails.createdBy);
    }, [props.eventDetails]);

    React.useEffect(() => { getUserProfile(props.eventDetails.createdBy); }, []);

    /**
     * Get event creator information
     * @param userId The user ID of which information to get
     */
    const getUserProfile = async (userId: string) => {
        let user: Array<string> = [userId];
        let response = await getUserProfiles(user);

        if (response.status === ResponseStatus.OK && response.data) {
            let userInfo = response.data[0];
            setCreatedByName(userInfo.displayName);
        }
    }

    /** Renders event audience */
    const getEventAudience = () => {
        if (eventDetails.audience === EventAudience.Private) {
            return <Label className="category-type-label" circular icon={<EyeSlashIcon />} iconPosition="start" content={localize("private")} />;
        }
        else {
            return <Label className="category-type-label" circular icon={<EyeIcon />} iconPosition="start" content={localize("public")} />;
        }
    }

    /**
     * Format and renders event day and time as per local time
     * @param startDate The start date of an event
     * @param startTime The start time of an event
     * @param endTime The end time of an event
     */
    const getEventDayAndTime = (startDate: Date, startTime: Date, endTime: Date) => {
        let eventDay = moment.utc(startDate).local().format("ddd");
        let eventStartTime = moment.utc(startTime).local().format("HH:mm");
        let eventEndTime = moment.utc(endTime).local().format("HH:mm");

        return `${eventDay}, ${eventStartTime} - ${eventEndTime}`;
    }

    /** Renders the event creator information */
    const renderEventCreatorInfo = () => {
        let name = createdByName && createdByName.length ? createdByName : localize("unknownUserName");
        return (
            <Flex vAlign="center" hAlign="start" gap="gap.smaller" design={{ marginTop: "2.67rem" }}>
                <Avatar
                    size="small"
                    name={name}
                />
                <Layout
                    start={<Text content={localize("createdBy")} size="small" />}
                    main={<Text content={name} size="small" weight="semibold" />}
                    gap=".4rem"
                />
            </Flex>
        );
    }

    /**
     * Renders event venue if event type is In-Person
     * @param eventType The event type
     * @param venue The event venue
     */
    const renderEventVenue = (eventType: number, venue: string) => {
        if (eventType === EventType.InPerson) {
            return (
                <React.Fragment>
                    <Divider vertical styles={{ height: "1.13rem", marginRight: ".8rem" }} />
                    <Text className="event-venue" content={venue} weight="semibold" size="small" />
                </React.Fragment>
            );
        }
    }

    /** Renders component */
    const renderEventDetails = () => {
        if (isLoading) {
            return (
                <Loader className="task-module-loader" />
            );
        }

        if (errorGettingEventDetails) {
            return <Text error content={localize("dataResponseFailedStatus")} weight="semibold" />
        }

        return (
            <div className="close-cancel-event-container">
                <div className="close-cancel-event">
                    <Flex space="between" vAlign="center">
                        <Text className="category label-color" content={eventDetails.categoryName} weight="bold" />
                        <Flex.Item push>
                            {getEventAudience()}
                        </Flex.Item>
                    </Flex>
                    <Image className="event-image" fluid src={eventDetails.photo} />
                    <div style={{ marginTop: "1.33rem" }}>
                        <Text content={eventDetails.name} weight="bold" size="medium" />
                    </div>
                    <Flex vAlign="center" hAlign="start">
                        <Layout
                            className="event-date-and-time"
                            start={<Icon iconName="Clock" />}
                            main={<Text content={getEventDayAndTime(eventDetails.startDate, eventDetails.startTime!, eventDetails.endTime!)} weight="semibold" size="small" />}
                            gap=".4rem"
                        />
                        {renderEventVenue(eventDetails.type, eventDetails.venue)}
                    </Flex>
                    <Flex vAlign="center" hAlign="start" design={{ marginTop: "2.67rem" }}>
                        <Text content={eventDetails.description} />
                    </Flex>
                    <Flex gap="gap.large" design={{ marginTop: "2.67rem" }}>
                        <Flex column>
                            <Text content={localize("totalNoOfParticipants")} weight="semibold" />
                            <Text content={eventDetails.maximumNumberOfParticipants} />
                        </Flex>
                        <Flex column>
                            <Text content={localize("registeredParticipants")} weight="semibold" />
                            <Text content={eventDetails.registeredAttendeesCount} />
                        </Flex>
                    </Flex>
                    {renderEventCreatorInfo()}
                </div>
            </div>
        );
    }

    return renderEventDetails();
}

export default EventDetails;