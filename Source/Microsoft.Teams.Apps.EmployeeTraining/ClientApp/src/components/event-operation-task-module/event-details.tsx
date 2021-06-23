// <copyright file="event-details.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Divider, Avatar, Layout, Provider, Button, Loader } from "@fluentui/react-northstar";
import { Icon } from '@fluentui/react/lib/Icon';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IEvent } from "../../models/IEvent";
import { EventAudience } from "../../models/event-audience";
import { EventType } from "../../models/event-type";
import { EventOperationType } from "../../models/event-operation-type";
import { formatEventDayAndTimeToShort } from "../../helpers/event-helper";
import AudienceArtifact from "../../components/common/event-artifacts/audience";
import TeamsMeetingArtifact from "../../components/common/event-artifacts/teams-meeting";
import LiveEventArtifact from "../../components/common/event-artifacts/live-event";
import MandatoryArtifact from "../../components/common/event-artifacts/mandatory";
import EventImage from "../../components/common/event-image/event-image";
import { LanguageDirection } from "../../models/language-direction";
import { Fabric } from "@fluentui/react";

import "./event-details.css";

interface IEventDetailsProps extends WithTranslation {
    dir: LanguageDirection
    eventDetails: IEvent | undefined,
    eventCreatedByName: string
    eventOperationType: EventOperationType,
    isLoadingEventDetails: boolean,
    isFailedToGetEventDetails: boolean,
    isOperationInProgress: boolean,
    isOperationFailed: boolean,
    isMobileView: boolean,
    onPerformOperation: () => void
}

/** Renders the event details in task module */
const EventDetails: React.FunctionComponent<IEventDetailsProps> = props => {
    const localize: TFunction = props.t;

    /** Renders artifacts */
    const getArtifacts = () => {
        let artifactElements: Array<JSX.Element> = [];

        if (props.eventDetails?.isMandatoryForLoggedInUser) {
            artifactElements.push(<MandatoryArtifact />);
        }

        switch (props.eventDetails?.type) {
            case EventType.Teams:
                artifactElements.push(<TeamsMeetingArtifact />);
                break;

            case EventType.LiveEvent:
                artifactElements.push(<LiveEventArtifact />);
                break;

            default:
                break;
        }

        switch (props.eventDetails?.audience) {
            case EventAudience.Private:
                artifactElements.push(<AudienceArtifact audienceType={EventAudience.Private} />);
                break;

            case EventAudience.Public:
                artifactElements.push(<AudienceArtifact audienceType={EventAudience.Public} />);
                break;

            default:
                break;
        }

        return (
            <Flex gap="gap.small" vAlign="center">
                {artifactElements}
            </Flex>
        );
    }

    /** Renders the event creator information */
    const renderEventCreatorInfo = () => {
        let name = props.eventCreatedByName && props.eventCreatedByName.length ? props.eventCreatedByName : localize("unknownUserName");
        return (
            <Flex vAlign="center" hAlign="start" gap="gap.smaller" design={{ marginTop: "2.67rem" }}>
                <Avatar
                    size="small"
                    name={name}
                    className={props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}
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

    /** Sets the event operation type that the task module is going to do */
    const renderOperationButtonText = () => {
        switch (props.eventOperationType) {
            case EventOperationType.CloseRegistration:
                return localize("closeRegistration");

            case EventOperationType.CancelEvent:
                return localize("cancelEvent");

            case EventOperationType.Register:
                return localize("register");

            case EventOperationType.Remove:
                return localize("remove");

            default:
                break;
        }
    }

    /** Renders the footer which has the submit action to close, cancel, register or cancel an event */
    const renderFooter = () => {
        if (props.eventOperationType === EventOperationType.None) {
            return;
        }

        return (
            <Flex className="footer" vAlign="center">
                {props.isOperationFailed ? <Text error content={localize("dataResponseFailedStatus")} weight="semibold" /> : null}
                <Flex.Item grow={props.dir === LanguageDirection.Rtl} push={props.dir === LanguageDirection.Ltr}>
                    <div >
                        <Button
                            primary
                            loading={props.isOperationInProgress}
                            disabled={props.isOperationInProgress}
                            content={renderOperationButtonText()}
                            onClick={props.onPerformOperation}
                        />
                    </div>
                </Flex.Item>
            </Flex>
        );
    }

    /** Renders task module header */
    const renderHeader = () => {
        if (props.isMobileView) {
            return (
                <Flex column vAlign="center" gap="gap.smaller">
                    <Text className="category label-color" truncated content={props.eventDetails?.categoryName} title={props.eventDetails?.categoryName} weight="bold" />
                    <Flex vAlign="center">
                        {getArtifacts()}
                    </Flex>
                </Flex>
            );
        }
        else {
            return (
                <Flex space="between" vAlign="center">
                    <Text className="category label-color" truncated content={props.eventDetails?.categoryName} title={props.eventDetails?.categoryName} weight="bold" />
                    <Flex.Item push={props.dir === LanguageDirection.Ltr}>
                        {getArtifacts()}
                    </Flex.Item>
                </Flex>
            );
        }
    }

    /** Renders the attendee URL for live event */
    const renderAttendeeURL = () => {
        if (props.eventDetails?.type === EventType.LiveEvent) {
            return (
                <Flex design={{ marginTop: "2.67rem" }} vAlign="center" hAlign="start" column>
                    <Text content={localize("liveEventUrlStep1")} weight="semibold" />
                    <a href={props.eventDetails?.meetingLink} target="_blank" rel="noopener noreferrer">{props.eventDetails?.meetingLink}</a>
                </Flex>
            );
        }
    }

    /** Renders component */
    const renderEventDetails = () => {
        if (props.isLoadingEventDetails) {
            return (
                <Provider>
                    <Flex>
                        <div className="task-module-container">
                            <Loader className="task-module-loader" />
                        </div>
                    </Flex>
                </Provider>
            );
        }

        if (props.isFailedToGetEventDetails || !props.eventDetails) {
            return (
                <Provider>
                    <Flex>
                        <div className="task-module-container event-task-module">
                            <Text error content={localize("dataResponseFailedStatus")} weight="semibold" />
                        </div>
                    </Flex>
                </Provider>
            );
        }

        return (
            <Fabric dir={props.dir}>
                <Provider>
                    <Flex>
                        <div className={`${props.isMobileView ? "mobile-task-module-container" : "task-module-container"} event-task-module`}>
                            <div className="event-info">
                                { renderHeader() }
                                {props.eventDetails.photo && <EventImage className="event-image" imageSrc={props.eventDetails.photo} />}
                                {props.eventDetails.selectedColor && <div className="event-image" style={{ backgroundColor: props.eventDetails.selectedColor }}>
                                    <Flex className="event-image" hAlign="center" vAlign="center">
                                        <Text className="event-color-text" size="large" weight="semibold" content={props.eventDetails.name} title={props.eventDetails.name} />
                                    </Flex>
                                </div>}
                                <div style={{ marginTop: "1.33rem" }}>
                                    <Text align={props.dir === LanguageDirection.Rtl ? "end" : "start"} className={props.dir === LanguageDirection.Rtl ? "rtl-right-margin-smaller" : ""} content={props.eventDetails?.name} weight="bold" size="medium" />
                                </div>
                                <Flex vAlign="center" hAlign="start">
                                    <Layout
                                        className="event-date-and-time"
                                        start={<Icon iconName="Clock" />}
                                        main={<Text content={formatEventDayAndTimeToShort(props.eventDetails?.startDate, props.eventDetails?.startTime!, props.eventDetails.endTime!)} weight="semibold" size="small" />}
                                        gap=".4rem"
                                    />
                                    {renderEventVenue(props.eventDetails?.type, props.eventDetails?.venue)}
                                </Flex>
                                <Flex vAlign="center" hAlign="start" design={{ marginTop: "2.67rem" }}>
                                    <Text className={props.dir === LanguageDirection.Rtl ? "rtl-right-margin-small" : ""} content={props.eventDetails?.description} />
                                </Flex>
                                { renderAttendeeURL() }
                                <Flex gap={props.dir === LanguageDirection.Rtl ? undefined : "gap.large"} className={props.dir === LanguageDirection.Rtl ? "rtl-right-margin-small" : ""} design={{ marginTop: "2.67rem" }}>
                                    <Flex column className={props.dir === LanguageDirection.Rtl ? "rtl-left-margin-large" : ""}>
                                        <Text content={localize("totalNoOfParticipants")} weight="semibold" />
                                        <Text align={props.dir === LanguageDirection.Rtl ? "end" : "start"} content={props.eventDetails?.maximumNumberOfParticipants} />
                                    </Flex>
                                    <Flex column>
                                        <Text content={localize("registeredParticipants")} weight="semibold" />
                                        <Text align={props.dir === LanguageDirection.Rtl ? "end" : "start"} content={props.eventDetails?.registeredAttendeesCount} />
                                    </Flex>
                                </Flex>
                                {renderEventCreatorInfo()}
                            </div>
                            {renderFooter()}
                        </div>
                    </Flex>
                </Provider>
            </Fabric>
        );
    }

    return renderEventDetails();
}

export default withTranslation()(EventDetails);