// <copyright file="create-event-step3.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { WithTranslation, withTranslation } from "react-i18next";
import { Text, Flex, Image, Button, ArrowLeftIcon, ArrowRightIcon, Layout, Divider } from '@fluentui/react-northstar'
import { TFunction } from "i18next";
import { Icon } from 'office-ui-fabric-react';
import { ICreateEventState } from "./create-event-wrapper";
import { EventType } from "../../models/event-type";
import moment from 'moment';
import { EventAudience } from "../../models/event-audience";
import { createEvent, saveEventAsDraftAsync, validateSelectedUsers, updateEventDetails } from "../../helpers/event-helper";
import withContext, { IWithContext } from "../../providers/context-provider";
import AudienceArtifact from "../../components/common/event-artifacts/audience";
import TeamsMeetingArtifact from "../../components/common/event-artifacts/teams-meeting";
import LiveEventArtifact from "../../components/common/event-artifacts/live-event";
import { LanguageDirection } from "../../models/language-direction";

interface ICreateEventsStep3Props extends WithTranslation, IWithContext {
    navigateToPage: (nextPage: number, stepEventState: ICreateEventState) => void;
    eventPageState: ICreateEventState;
}

interface ICreateEventsStep3State {
    isCreateLoading: boolean,
    isDraftLoading: boolean,
    registeredAttendeesCount: number
}

/** This component adds a new event category */
class CreateEventStep3 extends React.Component<ICreateEventsStep3Props, ICreateEventsStep3State> {
    readonly localize: TFunction;
    teamId: string;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.teamId = "";
        this.state = {
            isCreateLoading: false,
            registeredAttendeesCount: 0,
            isDraftLoading: false
        }
    }

    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.teamId = context.teamId!;
        });

        if (this.props.eventPageState.eventDetails.audience === EventAudience.Private && this.props.eventPageState.eventDetails.isAutoRegister) {
            this.getRegisteredAttendeesCount();
        }
    }

    /**
    * Sets the attendees count attending the event in state
    */
    getRegisteredAttendeesCount = async () => {
        let result = await validateSelectedUsers([...this.props.eventPageState.selectedUserGroups]);
        this.setState({ registeredAttendeesCount: result.mandatoryUsers.length });
    }

    /**
    * Event handler for moving onto previous step
    */
    backBtnClick = () => {
        this.props.navigateToPage(2, this.props.eventPageState);
    };

    /** Renders artifacts */
    getArtifacts = () => {
        let artifactElements: Array<JSX.Element> = [];

        switch (this.props.eventPageState.eventDetails?.type) {
            case EventType.Teams:
                artifactElements.push(<TeamsMeetingArtifact />);
                break;

            case EventType.LiveEvent:
                artifactElements.push(<LiveEventArtifact />);
                break;

            default:
                break;
        }

        switch (this.props.eventPageState.eventDetails?.audience) {
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

    /**
     * Format and renders event day and time as per local time
     * @param startDate The start date of an event
     * @param startTime The start time of an event
     * @param endTime The end time of an event
     */
    getEventDayAndTime = (startDate: Date, startTime: Date, endTime: Date) => {
        let eventDay = moment.utc(startDate).local().format("ddd");
        let eventStartTime = moment.utc(startTime).local().format("HH:mm");
        let eventEndTime = moment.utc(endTime).local().format("HH:mm");

        return `${eventDay}, ${eventStartTime} - ${eventEndTime}`;
    }

    /**
     * Renders event venue if event type is In-Person
     * @param eventType The event type
     * @param venue The event venue
     */
    renderEventVenue = (eventType: number, venue: string) => {
        if (eventType === EventType.InPerson) {
            return (
                <React.Fragment>
                    <Divider vertical styles={{ height: "1.13rem", marginRight: ".8rem" }} />
                    <Text className="event-venue" content={venue} weight="semibold" size="small" />
                </React.Fragment>
            );
        }
    }

    /**
    * Event handler for saving event as a draft
    */
    saveEventAsDraft = async () => {
        this.setState({ isDraftLoading: true });
        let result = await saveEventAsDraftAsync({ ...this.props.eventPageState }, this.teamId);
        if (result) {
            microsoftTeams.tasks.submitTask({ isSuccess: true, isDraft: true });
        }
        else {
            this.setState({ isDraftLoading: false });
            microsoftTeams.tasks.submitTask({ isSuccess: false, isDraft: true });
        }
    }

    /**
    * Event Handler for creating an event
    */
    createEvent = async () => {
        this.setState({ isCreateLoading: true });
        let result = await createEvent(this.props.eventPageState, this.teamId);
        if (result) {
            microsoftTeams.tasks.submitTask({ isSuccess: true, isCreateEvent: true });
        }
        else {
            this.setState({ isCreateLoading: false });
            microsoftTeams.tasks.submitTask({ isSuccess: false, isCreateEvent: true });
        }
    }

    /**
    * Updating an already created event and fetching its saved details
    */
    updateEvent = async () => {
        this.setState({ isCreateLoading: true });
        let result = await updateEventDetails(this.props.eventPageState, this.teamId);
        if (result) {
            microsoftTeams.tasks.submitTask({ isSuccess: true });
        }
        else {
            this.setState({ isCreateLoading: false });
            microsoftTeams.tasks.submitTask({ isSuccess: false });
        }
    }

    /** Renders the attendee URL for live event */
    renderAttendeeURL = () => {
        if (this.props.eventPageState.eventDetails.type === EventType.LiveEvent) {
            return (
                <Flex design={{ marginTop: "2.67rem" }} vAlign="center" hAlign="start" column>
                    <Text content={this.localize("liveEventUrlStep1")} weight="semibold" />
                    <a href={this.props.eventPageState.eventDetails.meetingLink} target="_blank" rel="noopener noreferrer">{this.props.eventPageState.eventDetails.meetingLink}</a>
                </Flex>
            );
        }
    }

    /** Renders a component */
    render() {
        return (
            <>
                <div className="page-content create-event-step3">
                    <Flex gap="gap.smaller">
                        <Text size="large" content={this.localize("eventPreviewStep3")} />
                    </Flex>
                    <Flex className="margin-top" space="between" vAlign="center">
                        <Text className="category label-color" content={this.props.eventPageState.selectedCategory?.header!} weight="bold" />
                        <Flex.Item push={this.props.dir === LanguageDirection.Ltr}>
                            {this.getArtifacts()}
                        </Flex.Item>
                    </Flex>
                    {this.props.eventPageState.eventDetails.photo && <Image className="event-image" fluid src={this.props.eventPageState.eventDetails.photo} />}
                    {this.props.eventPageState.eventDetails.selectedColor && <div className="event-image" style={{ backgroundColor: this.props.eventPageState.eventDetails.selectedColor }}>
                        <Flex className="event-image-color" hAlign="center" vAlign="center">
                            <Text className="event-color-text" size="large" weight="semibold" content={this.props.eventPageState.eventDetails.name} />
                        </Flex>
                    </div>}
                    <div style={{ marginTop: "1.33rem" }}>
                        <Text align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.props.eventPageState.eventDetails.name} weight="bold" size="medium" />
                    </div>
                    <Flex vAlign="center" hAlign="start">
                        <Layout
                            className="event-date-and-time"
                            start={<Icon iconName="Clock" />}
                            main={<Text content={this.getEventDayAndTime(this.props.eventPageState.eventDetails.startDate, this.props.eventPageState.eventDetails.startTime!, this.props.eventPageState.eventDetails.endTime!)} weight="semibold" size="small" />}
                            gap=".4rem"
                        />
                        {this.renderEventVenue(this.props.eventPageState.eventDetails.type, this.props.eventPageState.eventDetails.venue)}
                    </Flex>
                    <Flex vAlign="center" hAlign="start" design={{ marginTop: "2.67rem" }}>
                        <Text align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.props.eventPageState.eventDetails.description} />
                    </Flex>
                    {this.renderAttendeeURL()}
                    <Flex gap={this.props.dir === LanguageDirection.Rtl ? undefined : "gap.large"} design={{ marginTop: "2.67rem" }}>
                        <Flex column className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}>
                            <Text align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.localize("totalNoOfParticipants")} weight="semibold" />
                            <Text align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.props.eventPageState.eventDetails.maximumNumberOfParticipants} />
                        </Flex>
                        <Flex column>
                            <Text align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.localize("registeredParticipants")} weight="semibold" />
                            <Text align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.state.registeredAttendeesCount} />
                        </Flex>
                    </Flex>
                </div>
                {this.props.dir === LanguageDirection.Ltr && <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    <Button disabled={this.state.isCreateLoading} icon={<ArrowLeftIcon />} text content={this.localize("back")} onClick={this.backBtnClick} />
                    <Flex.Item push>
                        <Text weight="bold" content={this.localize("step3of3")} />
                    </Flex.Item>
                    {
                        (this.props.eventPageState.isDraft || !this.props.eventPageState.isEdit) && <>
                            <Button disabled={this.state.isCreateLoading || this.state.isDraftLoading} loading={this.state.isDraftLoading} onClick={this.saveEventAsDraft} content={this.localize("saveAsDraft")} secondary data-testid="save_button" />
                            <Button disabled={this.state.isCreateLoading || this.state.isDraftLoading} loading={this.state.isCreateLoading} content={this.localize("createEvent")} primary onClick={this.createEvent} data-testid="create_event_button" />
                        </>
                    }
                    {
                        !this.props.eventPageState.isDraft && this.props.eventPageState.isEdit && <>
                            <Button disabled={this.state.isCreateLoading} loading={this.state.isCreateLoading} content={this.localize("updateEventButton")} primary onClick={this.updateEvent} data-testid="update_button" />
                        </>
                    }
                </Flex>}

                {this.props.dir === LanguageDirection.Rtl && <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    <Flex.Item push>
                        <Button
                            disabled={this.state.isCreateLoading}
                            icon={<ArrowRightIcon />}
                            text
                            content={<Text content={this.localize("back")} className="rtl-right-margin-small" />}
                            onClick={this.backBtnClick}
                        />
                    </Flex.Item>
                    
                    <Text className="rtl-left-margin-small" weight="bold" content={this.localize("step3of3")} />
                    {
                        (this.props.eventPageState.isDraft || !this.props.eventPageState.isEdit) && <>
                            <Button className="rtl-left-margin-small" disabled={this.state.isCreateLoading || this.state.isDraftLoading} loading={this.state.isDraftLoading} onClick={this.saveEventAsDraft} content={this.localize("saveAsDraft")} secondary data-testid="save_button" />
                            <Button disabled={this.state.isCreateLoading || this.state.isDraftLoading} loading={this.state.isCreateLoading} content={this.localize("createEvent")} primary onClick={this.createEvent} data-testid="create_event_button" />
                        </>
                    }
                    {
                        !this.props.eventPageState.isDraft && this.props.eventPageState.isEdit && <>
                            <Button className="rtl-left-margin-small" disabled={this.state.isCreateLoading} loading={this.state.isCreateLoading} content={this.localize("updateEventButton")} primary onClick={this.updateEvent} data-testid="update_button" />
                        </>
                    }
                </Flex>}
            </>
        );
    }
}

export default withTranslation()(withContext(CreateEventStep3));