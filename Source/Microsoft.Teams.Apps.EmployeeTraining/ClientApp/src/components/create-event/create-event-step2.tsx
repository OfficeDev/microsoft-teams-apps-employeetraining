// <copyright file="create-event-step2.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { WithTranslation, withTranslation } from "react-i18next";
import DropdownSearch, { IDropdownItem } from "../common/user-search-dropdown/dropdown-search";
import { Text, Flex, Button, Dropdown, Checkbox, Table } from '@fluentui/react-northstar'
import { TrashCanIcon, QuestionCircleIcon, InfoIcon, ArrowLeftIcon, ArrowRightIcon } from '@fluentui/react-icons-northstar';
import { TFunction } from "i18next";
import { getLocalizedAudienceTypes } from "../../helpers/localized-constants";
import { IConstantDropdownItem } from "../../constants/resources";
import { ISelectedUserGroup } from "../../models/ISelectedUserGroup";
import { ICreateEventState } from "./create-event-wrapper";
import { IEvent } from "../../models/IEvent";
import { ISelectedDropdownItem } from "../../models/ISelectedDropdownItem";
import { EventAudience } from "../../models/event-audience";
import { saveEventAsDraftAsync } from "../../helpers/event-helper";
import { LanguageDirection } from "../../models/language-direction";

interface ICreateEventsStep2Props extends WithTranslation {
    navigateToPage: (nextPage: number, stepEventState: ICreateEventState) => void;
    eventPageState: ICreateEventState;
    dir: LanguageDirection;
}

interface ICreateEventsStep2State {
    selectedUsersAndGroups: Array<ISelectedUserGroup>,
    eventDetails: IEvent,
    selectedAudienceType: ISelectedDropdownItem,
    audienceTypes: Array<IConstantDropdownItem>,
    isLoading: boolean
}

/** This component adds a new event category */
class CreateEventStep2 extends React.Component<ICreateEventsStep2Props, ICreateEventsStep2State> {
    readonly localize: TFunction;
    teamId: string;

    constructor(props: any) {
        super(props);
        this.teamId = "";
        this.localize = this.props.t;
        let audienceTypes = getLocalizedAudienceTypes(this.localize);
        this.state = {
            isLoading: false,
            selectedAudienceType: { key: this.props.eventPageState.eventDetails.audience?.toString()!, header: audienceTypes.find((audience) => audience.id === this.props.eventPageState.eventDetails.audience!)?.name! },
            selectedUsersAndGroups: this.props.eventPageState.selectedUserGroups.length > 0 ? this.props.eventPageState.selectedUserGroups : new Array<ISelectedUserGroup>(),
            eventDetails: this.props.eventPageState.eventDetails,
            audienceTypes: audienceTypes
        }
    }

    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.teamId = context.teamId!;
        });
    }

    /**
    * Event handler for moving onto next event-step
    */
    nextBtnClick = () => {
        let modifiedState = { ...this.props.eventPageState };
        modifiedState.selectedUserGroups = this.state.selectedUsersAndGroups;
        modifiedState.selectedAudience = this.state.selectedAudienceType;
        modifiedState.eventDetails = this.state.eventDetails;
        modifiedState.eventDetails.selectedUserOrGroupListJSON = JSON.stringify(this.state.selectedUsersAndGroups);
        this.props.navigateToPage(3, modifiedState);
    };

    /**
    *  Event handler for moving onto previous event-step
    */
    backBtnClick = () => {
        let modifiedState = { ...this.props.eventPageState };
        modifiedState.selectedUserGroups = this.state.selectedUsersAndGroups;
        modifiedState.selectedAudience = this.state.selectedAudienceType;
        modifiedState.eventDetails = this.state.eventDetails;
        modifiedState.eventDetails.selectedUserOrGroupListJSON = JSON.stringify(this.state.selectedUsersAndGroups);
        this.props.navigateToPage(1, modifiedState);
    };

    /**
    * Updating member list for mandatory option change event
    * @param memberIndex array index of a specific member in member list
    */
    onToggleChange = (memberIndex: number) => {
        let members = this.state.selectedUsersAndGroups;
        members[memberIndex].isMandatory = !members[memberIndex].isMandatory;

        this.setState({ selectedUsersAndGroups: members });
    }

    /**
    * Removing a member from selected member list for an event
    * @param memberIndex array index of a specific member in member list
    */
    deleteItem = (memberIndex: number) => {
        let members = this.state.selectedUsersAndGroups;
        members.splice(memberIndex, 1);

        this.setState({ selectedUsersAndGroups: members });
    }

    /**
    * Fetched members through api call are rendered in a component 
    */
    renderMembers = () => {
        if (this.state.selectedUsersAndGroups && this.state.selectedUsersAndGroups.length > 0) {

            let rows = this.state.selectedUsersAndGroups.map((member, index) => {
                return {
                    "key": index,
                    "items": [
                        {
                            content: <>
                                <Text weight="bold" content={member.displayName} /><br />
                                <Text size="small" content={member.email} />
                            </>,
                            title: member.displayName,
                            truncateContent: true
                        },
                        {
                            content: <>
                                    <Text content={this.localize("mandatoryToggleStep2")}/>
                                    <Checkbox 
                                        className={this.props.dir === LanguageDirection.Rtl ? "rtl-toggle" : ""}
                                        onChange={() => this.onToggleChange(index)}
                                        checked={member.isMandatory}
                                        toggle
                                    />
                                </>,
                            className: "mandatory-toggle-column"
                        },
                        {
                            content: <TrashCanIcon className="icon-hover" onClick={() => this.deleteItem(index)} />,
                            className: "delete-button-column"
                        }
                    ]
                }
            });

            return (
                <Table className="selected-user-group-table" rows={rows} />
            );
        }
        else {
            return (
                <Flex gap="gap.small">
                    <Flex.Item>
                        <div
                            style={{
                                position: "relative",
                            }}
                        >
                            <QuestionCircleIcon outline color="green" />
                        </div>
                    </Flex.Item>
                    <Flex.Item grow={this.props.dir === LanguageDirection.Ltr}>
                        <Flex column gap="gap.small" vAlign="stretch" className={this.props.dir === LanguageDirection.Rtl ? "rtl-right-margin-medium rtl-direction" : ""}>
                            <div>
                                <Text weight="bold" content={this.localize("noUserSelectedHeaderStep2")} /><br />
                                <Text size="small" content={this.localize("noUserSelectedContentStep2")}
                                />
                            </div>
                        </Flex>
                    </Flex.Item>
                </Flex>
            );
        }
    }

    /**
    * Event handler for selecting audience type
    */
    onAudienceTypeSelection = {
        onAdd: (item: any) => {
            this.setState({ selectedAudienceType: item });
            return "";
        }
    }

    /**
    * Event handler for selecting users/group as an event audience
    * @param selectedItem selected value of an user/group
    */
    onUserOrGroupSelection = async (selectedItem: IDropdownItem) => {
        let selectedUserOrGroup: ISelectedUserGroup = {
            displayName: selectedItem.header,
            email: selectedItem.content,
            id: selectedItem.id,
            isGroup: selectedItem.isGroup,
            isMandatory: true
        };

        let existingUsers = this.state.selectedUsersAndGroups;
        let isAlreadyExist = existingUsers.find((userOrGroup) => userOrGroup.id === selectedUserOrGroup.id);
        if (!isAlreadyExist) {
            existingUsers.push(selectedUserOrGroup);
            this.setState({ selectedUsersAndGroups: existingUsers });
        }
    }

    /**
    * Event Handler for audience type dropdown
    * @param item selected audience type
    */
    onAudienceChange = (item: any) => {
        this.setState((prevState: ICreateEventsStep2State) => ({
            eventDetails: { ...prevState.eventDetails, audience: item.key },
            selectedAudienceType: item
        }));
    }

    /**
    * Event handler for auto-registering the mandatory users for an event
    */
    onAutoRegisterToggleChange = () => {
        this.setState((prevState: ICreateEventsStep2State) => ({
            eventDetails: { ...prevState.eventDetails, isAutoRegister: !this.state.eventDetails.isAutoRegister }
        }));
    }

    /**
    * Event handler for saving event as a draft
    */
    saveEventAsDraft = async () => {
        this.setState({ isLoading: true });
        let modifiedState = { ...this.props.eventPageState };
        modifiedState.selectedUserGroups = this.state.selectedUsersAndGroups;
        modifiedState.selectedAudience = this.state.selectedAudienceType;
        modifiedState.eventDetails = this.state.eventDetails;
        modifiedState.eventDetails.selectedUserOrGroupListJSON = JSON.stringify(this.state.selectedUsersAndGroups);

        let result = await saveEventAsDraftAsync(modifiedState, this.teamId);
        if (result) {
            microsoftTeams.tasks.submitTask({ isSuccess: true, isDraft: true });
        }
        else {
            this.setState({ isLoading: false });
        }
    }

    /**
    * Event handler for selecting mandatory all option
    */
    onMandatoryAllClocked = () => {
        let selectedUserGroup = [...this.state.selectedUsersAndGroups];
        for (var i = 0; i < selectedUserGroup.length; i++) {
            selectedUserGroup[i].isMandatory = true;
        }

        this.setState({ selectedUsersAndGroups: selectedUserGroup });
    }

    /** Renders a component */
    render() {
        return (
            <React.Fragment>
                <div className="page-content">
                    <Flex gap="gap.smaller">
                        <Text size="large" content={this.localize("audienceDetailsStep2")} />
                    </Flex>
                    <Flex gap="gap.smaller" className="margin-top">
                        <Flex.Item size="size.half">
                            <Flex gap="gap.smaller" vAlign="center">
                                <Text className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small form-label" : "form-label"} content={this.localize("audienceTypeStep2")} />
                                <InfoIcon outline title={this.localize("audienceTypeInfoIconTitle")} />
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    <Flex gap="gap.smaller" className="input-label-margin-between">
                        <Flex.Item size="size.half">
                            <Dropdown
                                fluid
                                value={this.state.selectedAudienceType}
                                items={this.state.audienceTypes.map((value: IConstantDropdownItem) => { return { key: value.id, header: value.name } })}
                                onChange={(event, data) => { this.onAudienceChange(data.value) }}
                                data-testid="event_audience_dropdown"
                            />
                        </Flex.Item>
                    </Flex>
                    {this.state.eventDetails.audience === EventAudience.Private && <>
                        <Flex gap="gap.smaller" className="margin-top">
                            <DropdownSearch
                                loadingMessage={this.localize("dropdownSearchLoadingMessage")}
                                noResultMessage={this.localize("noResultFoundDropdownMessage")}
                                placeholder={this.localize("startTypingDropdownSearch")}
                                onItemSelect={this.onUserOrGroupSelection}
                            />
                        </Flex>
                        <Flex gap="gap.smaller" className="input-label-margin-between">
                            <Checkbox onChange={() => this.onAutoRegisterToggleChange()} checked={this.state.eventDetails.isAutoRegister} label={this.localize("autoRegisterCheckboxLabelStep2")} data-testid="auto_toggle" />
                            <Flex.Item push={this.props.dir === LanguageDirection.Ltr} grow={this.props.dir === LanguageDirection.Rtl}>
                                <Button
                                    onClick={this.onMandatoryAllClocked}
                                    primary
                                    text
                                    content={this.localize("mandatoryAllButtonStep2")}
                                    disabled={this.state.selectedUsersAndGroups.filter((userOrGroup) => userOrGroup.isMandatory === false).length === 0}
                                    data-testid="audience_mandatory_button"
                                    className={this.props.dir === LanguageDirection.Rtl ? "left-push" : "" }
                                />
                            </Flex.Item>
                        </Flex>
                        {this.renderMembers()}
                    </>}
                </div>
                {this.props.dir === LanguageDirection.Ltr && <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    <Button icon={<ArrowLeftIcon />} text content={this.localize("back")} onClick={this.backBtnClick} data-testid="back_button" />
                    <Flex.Item push>
                        <Text weight="bold" content={this.localize("step2Of3")} />
                    </Flex.Item>
                    {(!this.props.eventPageState.isEdit || (this.props.eventPageState.isEdit && this.props.eventPageState.isDraft)) && <Button disabled={this.state.isLoading} loading={this.state.isLoading} onClick={this.saveEventAsDraft} content={this.localize("saveAsDraft")} secondary data-testid="save_draft_button" />}
                    <Button content={this.localize("nextButton")} disabled={this.state.isLoading || (this.state.eventDetails.audience === EventAudience.Private && this.state.selectedUsersAndGroups.length === 0)} primary onClick={this.nextBtnClick} data-testid="next_button" />
                </Flex>}

                {this.props.dir === LanguageDirection.Rtl && <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    <Flex.Item push>
                        <Button icon={<ArrowRightIcon />} text content={<Text content={this.localize("back")} className={this.props.dir === LanguageDirection.Rtl ? "rtl-right-margin-small" : "" }/>} onClick={this.backBtnClick} data-testid="back_button" />
                    </Flex.Item>
                    <Text className="rtl-left-margin-small" weight="bold" content={this.localize("step2Of3")} />
                    {(!this.props.eventPageState.isEdit || (this.props.eventPageState.isEdit && this.props.eventPageState.isDraft)) && <Button className="rtl-right-margin-small" disabled={this.state.isLoading} loading={this.state.isLoading} onClick={this.saveEventAsDraft} content={this.localize("saveAsDraft")} secondary data-testid="save_draft_button" />}
                    <Button content={this.localize("nextButton")} disabled={this.state.isLoading || (this.state.eventDetails.audience === EventAudience.Private && this.state.selectedUsersAndGroups.length === 0)} primary onClick={this.nextBtnClick} data-testid="next_button" />
                </Flex>}
            </React.Fragment>
        );
    }
}
export default withTranslation()(CreateEventStep2);