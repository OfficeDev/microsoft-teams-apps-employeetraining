// <copyright file="event-card.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Divider } from "@fluentui/react-northstar";
import moment from "moment";
import { Icon } from 'office-ui-fabric-react';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IEvent } from "../../models/IEvent";
import MandatoryArtifact from "../common/event-artifacts/mandatory";
import TeamsMeetingArtifact from "../common/event-artifacts/teams-meeting";
import AudienceArtifact from "../common/event-artifacts/audience";
import LiveEventArtifact from "../common/event-artifacts/live-event";
import { EventAudience } from "../../models/event-audience";
import { formatEventDayAndTimeToShort } from "../../helpers/event-helper";
import EventImage from "../event-image/event-image";
import { EventType } from "../../models/event-type";
import withContext, { IWithContext } from "../../providers/context-provider";

import "../event-card/event-card.css";

interface IEventCardProps extends WithTranslation, IWithContext {
    eventDetails: IEvent,
    onClick: () => void
}

/**
 * Renders the event card for user
 * @param props The props with type IEventCard
 */
const EventCard: React.FunctionComponent<IEventCardProps> = props => {
    const localize: TFunction = props.t;

    const renderFooter = () => {
        let footerElements: Array<JSX.Element> = [];

        if (props.eventDetails.isMandatoryForLoggedInUser) {
            footerElements.push(<MandatoryArtifact />);
        }

        switch (props.eventDetails.type) {
            case EventType.Teams:
                footerElements.push(<TeamsMeetingArtifact />);
                break;

            case EventType.LiveEvent:
                footerElements.push(<LiveEventArtifact />);
                break;

            default:
                break;
        }

        switch (props.eventDetails.audience) {
            case EventAudience.Private:
                footerElements.push(<AudienceArtifact audienceType={EventAudience.Private} />);
                break;

            case EventAudience.Public:
                footerElements.push(<AudienceArtifact audienceType={EventAudience.Public} />);
                break;

            default:
                break;
        }

        return (
            <Flex className="footer" gap="gap.small" vAlign="center">
                {footerElements}
            </Flex>
        );
    }

    /** Gets the available slots for the event */
    const getAvailableSlots = () => {
        if (props.eventDetails.registeredAttendeesCount < props.eventDetails.maximumNumberOfParticipants) {
            return props.eventDetails.maximumNumberOfParticipants - props.eventDetails.registeredAttendeesCount;
        }
        else {
            return 0;
        }
    }

    /** Renders event venue if event type is In-Person */
    const renderEventVenue = () => {
        if (props.eventDetails.type === EventType.InPerson) {
            return (
                <React.Fragment>
                    <Divider className="event-date-venue-separator" vertical />
                    <Text truncated className="event-venue" content={props.eventDetails.venue} title={props.eventDetails.venue} weight="semibold" size="small" />
                </React.Fragment>
            );
        }
    }

    return (
        <Flex column className="event-card" onClick={props.onClick}>
            {props.eventDetails.photo && <EventImage className="event-photo" imageSrc={props.eventDetails.photo} />}
            {
                props.eventDetails.selectedColor &&
                <div className="event-photo" style={{ backgroundColor: props.eventDetails.selectedColor }}>
                    <Flex className="event-photo" hAlign="center" vAlign="center">
                        <Text className="event-color-text" size="medium" weight="semibold" title={props.eventDetails.name} content={props.eventDetails.name} />
                    </Flex>
                </div>
            }
            <Flex className="event-info" column fill>
                <Flex vAlign="center" gap="gap.medium">
                    <Flex className="event-day" column vAlign="center" hAlign="center" fill>
                        <Text content={moment.utc(props.eventDetails.startDate).local().format("DD")} size="large" weight="bold" />
                        <Text content={moment.utc(props.eventDetails.startDate).local().format("MMM")} size="medium" weight="semibold" />
                    </Flex>
                    <Flex className="event-details" column vAlign="center" fill>
                        <Text align={props.dir === "rtl" ? "end" : undefined} className={props.dir === "rtl" ? "event-category rtl-right-margin-small" : "event-category"} truncated content={props.eventDetails.categoryName} weight="bold" size="smaller" />
                        <Text align={props.dir === "rtl" ? "end" : undefined} className={props.dir === "rtl" ? "rtl-right-margin-small" : ""} truncated content={props.eventDetails.name} title={props.eventDetails.name} weight="bold" size="medium" />
                        <Flex className={props.dir === "rtl" ? "event-date-and-venue-rtl event-date-and-venue" : "event-date-and-venue"} vAlign="center">
                            <Icon iconName="Clock" />
                            <Flex.Item shrink={false}>
                                <Text weight="semibold" size="small" content={formatEventDayAndTimeToShort(props.eventDetails.startDate, props.eventDetails.startTime!, props.eventDetails.endTime!)} />
                            </Flex.Item>
                            {renderEventVenue()}
                        </Flex>
                    </Flex>
                </Flex>
                <Flex className="event-participants" gap="gap.small">
                    <Text size="small" content={`${localize("participantsRegistered")} : ${props.eventDetails.registeredAttendeesCount}`} />
                    <Divider vertical />
                    <Text size="small" content={`${localize("availableSlots")} : ${getAvailableSlots()}`} />
                </Flex>
                {renderFooter()}
            </Flex>
        </Flex>
    );
}

export default withTranslation()(withContext(EventCard));