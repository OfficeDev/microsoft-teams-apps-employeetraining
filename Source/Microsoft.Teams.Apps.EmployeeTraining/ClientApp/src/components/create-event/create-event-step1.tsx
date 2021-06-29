// <copyright file="create-event-step1.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { Button, Dropdown, ExclamationCircleIcon, FilesUploadIcon, Flex, Input, Text, TextArea, Image as FluentImage } from '@fluentui/react-northstar';
import moment from "moment-timezone";
import { TFunction } from "i18next";
import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { uploadEventImage, searchEventAsync } from "../../api/create-event-api";
import { ResponseStatus } from "../../constants/constants";
import Resources, { IConstantDropdownItem } from "../../constants/resources";
import { saveEventAsDraftAsync } from "../../helpers/event-helper";
import { getLocalizedEventTypes } from "../../helpers/localized-constants";
import { EventType } from "../../models/event-type";
import { IEvent } from "../../models/IEvent";
import { ISelectedDropdownItem } from "../../models/ISelectedDropdownItem";
import StartDate from "../common/date-picker/datepicker";
import { TimePicker } from "../common/time-picker/timepicker";
import { ICreateEventState } from "./create-event-wrapper";
import withContext, { IWithContext } from "../../providers/context-provider";
import { ColorPicker } from "../common/color-picker/color-picker";
import { LanguageDirection } from '../../models/language-direction';

interface ICreateEventsStep1Props extends WithTranslation, IWithContext {
    navigateToPage: (nextPage: number, stepEventState: ICreateEventState) => void;
    eventPageState: ICreateEventState;
}

interface ICreateEventsStep1State {
    theme: string,
    screenWidth: number,
    projectStartDate: number,
    isPhotoValid: boolean,
    isColorValid: boolean,
    isPhotoDimentionsValid: boolean,
    isNameValid: boolean,
    isEventNameExisting:boolean,
    isMeetingLinkValid: boolean,
    isDescriptionValid: boolean,
    isVenueValid: boolean,
    isEventTypeValid: boolean,
    isEventCategoryValid: boolean,
    isMaxNoofParticipantsValid: boolean,
    isTimeValid: boolean,
    eventDetails: IEvent,
    eventTypes: Array<IConstantDropdownItem>,
    categories: Array<IConstantDropdownItem>,
    selectedEventType: ISelectedDropdownItem,
    selectedCategory: ISelectedDropdownItem,
    imageUploadLoader: boolean,
    isLoading: boolean,
    isValidatingStep1: boolean,
    inputKey: number
}

/** This component adds a new event category */
class CreateEventStep1 extends React.Component<ICreateEventsStep1Props, ICreateEventsStep1State> {
    readonly localize: TFunction;
    // Create a reference to the hidden file input element
    inputReference: any;
    teamId: string;
    timeZone: string;

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        let eventTypes = getLocalizedEventTypes(this.localize);
        window.addEventListener("resize", this.update);
        this.inputReference = React.createRef();
        let date = new Date();
        this.teamId = "";
        this.timeZone = moment.tz.guess();

        this.state = {
            imageUploadLoader: false,
            inputKey: 1,
            isLoading: false,
            categories: this.props.eventPageState.categories.length > 0 ? this.props.eventPageState.categories : new Array<IConstantDropdownItem>(),
            theme: "",
            eventTypes: eventTypes,
            screenWidth: window.innerWidth,
            projectStartDate: date.setDate(date.getDate() + 1),
            isPhotoValid: true,
            isColorValid: true,
            isEventNameExisting: false,
            isPhotoDimentionsValid: true,
            isNameValid: true,
            isDescriptionValid: true,
            isTimeValid: true,
            isVenueValid: true,
            isMeetingLinkValid: true,
            isEventTypeValid: true,
            isEventCategoryValid: true,
            isMaxNoofParticipantsValid: true,
            isValidatingStep1: false,
            selectedEventType: this.props.eventPageState.eventDetails.type === undefined ? {
                key: EventType.Teams.toString(), header: eventTypes.find((event) => event.id === EventType.Teams)!.name!
            } : {
                    key: eventTypes.find((event) => event.id === this.props.eventPageState.eventDetails.type)!.id.toString(), header: eventTypes.find((event) => event.id === this.props.eventPageState.eventDetails.type)!.name!
                },
            selectedCategory: this.props.eventPageState.selectedCategory!,
            eventDetails: { ...this.props.eventPageState.eventDetails }
        }
    }

    UNSAFE_componentWillReceiveProps(nextProps: ICreateEventsStep1Props) {
        if (nextProps.eventPageState.categories !== this.props.eventPageState.categories) {
            this.setState({ categories: nextProps.eventPageState.categories });
        }

        if (nextProps.eventPageState.eventDetails !== this.props.eventPageState.eventDetails) {
            this.setState({ eventDetails: { ...nextProps.eventPageState.eventDetails } });
        }

        if (nextProps.eventPageState.selectedCategory !== this.props.eventPageState.selectedCategory) {
            this.setState({ selectedCategory: nextProps.eventPageState.selectedCategory! });
        }

        if (nextProps.teamsContext && nextProps.teamsContext !== this.props.teamsContext) {
            this.setState({ theme: nextProps.teamsContext.theme! });
            this.teamId = nextProps.teamsContext.teamId!;
        }
    }

    /** Update the screen width for screen resize */
    update = () => {
        this.setState({
            screenWidth: window.innerWidth
        });
    };

    /**
    * Function for applying validation on the fields for save as draft functionality
    */
    checkIfSaveAsDraftAllowed = () => {
        let eventValidationStatus = { isPhotoValid: true, isColorValid: true, isNameValid: true, isDescriptionValid: true, isEventTypeValid: true, isEventCategoryValid: true, isVenueValid: true, isMaxNoofPartipantsValid: true, isMeetingLinkValid: true };

        if (this.state.eventDetails.name == "" || this.state.eventDetails.name.length > Resources.eventNameMaxLength) {
            eventValidationStatus.isNameValid = false;
        }

        this.setState({
            isPhotoValid: eventValidationStatus.isPhotoValid,
            isColorValid: eventValidationStatus.isColorValid,
            isNameValid: eventValidationStatus.isNameValid,
            isDescriptionValid: eventValidationStatus.isDescriptionValid,
            isEventTypeValid: eventValidationStatus.isEventTypeValid,
            isEventCategoryValid: eventValidationStatus.isEventCategoryValid,
            isVenueValid: eventValidationStatus.isVenueValid,
            isMaxNoofParticipantsValid: eventValidationStatus.isMaxNoofPartipantsValid,
            isMeetingLinkValid: eventValidationStatus.isMeetingLinkValid
        });

        if (eventValidationStatus.isPhotoValid && eventValidationStatus.isColorValid && eventValidationStatus.isNameValid &&
            eventValidationStatus.isDescriptionValid && eventValidationStatus.isEventTypeValid &&
            eventValidationStatus.isEventCategoryValid && eventValidationStatus.isVenueValid &&
            eventValidationStatus.isMaxNoofPartipantsValid && eventValidationStatus.isMeetingLinkValid &&
            !this.state.isEventNameExisting) {
            return true;
        }
        else {
            return false;
        }
    }

    /**
    * Function for applying validation on the fields before moving onto next step in event creation
    */
    checkIfNextAllowed = async () => {
        let eventValidationStatus = { isTimeValid: true, isPhotoValid: true, isColorValid: true, isNameValid: true, isDescriptionValid: true, isEventTypeValid: true, isEventCategoryValid: true, isVenueValid: true, isMaxNoofPartipantsValid: true, isMeetingLinkValid: true };

        if (!this.state.eventDetails.photo && !this.state.eventDetails.selectedColor) {
            eventValidationStatus.isPhotoValid = false;
            eventValidationStatus.isColorValid = false;
        }

        if (this.state.eventDetails.name.trim() == "" || this.state.eventDetails.name.length > Resources.eventNameMaxLength) {
            eventValidationStatus.isNameValid = false;
        }

        let isEventNameAlreadyExists: boolean = false;

        if (eventValidationStatus.isNameValid) {
            // Checking whether event name already exists-
            // - creating a new event
            // - updating a event after name changed
            if (!this.props.eventPageState.isEdit || (this.props.eventPageState.eventDetails.name !== this.state.eventDetails.name.trim())) {
                isEventNameAlreadyExists = await this.checkEventName();
            }
        }

        if (this.state.eventDetails.description.trim() == "" || this.state.eventDetails.description.length > Resources.eventDescriptionMaxLength) {
            eventValidationStatus.isDescriptionValid = false;
        }
        if (!this.state.eventDetails.type) {
            eventValidationStatus.isEventTypeValid = false;
        }
        if (!this.state.eventDetails.categoryId) {
            eventValidationStatus.isEventCategoryValid = false;
        }
        if (this.state.eventDetails.type === EventType.InPerson && this.state.eventDetails.venue == "") {
            eventValidationStatus.isVenueValid = false;
        }
        if (!this.state.eventDetails.maximumNumberOfParticipants) {
            eventValidationStatus.isMaxNoofPartipantsValid = false;
        }
        if (this.state.eventDetails.type === EventType.LiveEvent && (this.state.eventDetails.meetingLink == "" || !this.state.eventDetails.meetingLink.match(Resources.validUrlRegExp))) {
            eventValidationStatus.isMeetingLinkValid = false;
        }

        if (this.checkEventAlreadyStarted()) {
            if ((!this.state.eventDetails!.startTime || !this.state.eventDetails!.endTime) || (this.state.eventDetails!.startTime! >= this.state.eventDetails!.endTime!)) {
                eventValidationStatus.isTimeValid = false;
            }
        }

        this.setState({
            isPhotoValid: eventValidationStatus.isPhotoValid,
            isColorValid: eventValidationStatus.isColorValid,
            isNameValid: eventValidationStatus.isNameValid,
            isDescriptionValid: eventValidationStatus.isDescriptionValid,
            isEventTypeValid: eventValidationStatus.isEventTypeValid,
            isEventCategoryValid: eventValidationStatus.isEventCategoryValid,
            isVenueValid: eventValidationStatus.isVenueValid,
            isMaxNoofParticipantsValid: eventValidationStatus.isMaxNoofPartipantsValid,
            isMeetingLinkValid: eventValidationStatus.isMeetingLinkValid,
            isTimeValid: eventValidationStatus.isTimeValid,
            isEventNameExisting: isEventNameAlreadyExists,
            isValidatingStep1: false
        });

        if (eventValidationStatus.isPhotoValid && eventValidationStatus.isColorValid && eventValidationStatus.isNameValid &&
            eventValidationStatus.isDescriptionValid && eventValidationStatus.isEventTypeValid &&
            eventValidationStatus.isEventCategoryValid && eventValidationStatus.isVenueValid &&
            eventValidationStatus.isMaxNoofPartipantsValid && eventValidationStatus.isMeetingLinkValid &&
            eventValidationStatus.isTimeValid && !this.state.isEventNameExisting) {
            return true;
        }
        else {
            return false;
        }
    }

    /** Check whether event start date is less than current date while editing an event */
    checkEventAlreadyStarted = () => {
        return !(this.props.eventPageState.isEdit && moment.utc(this.state.eventDetails.startDate).local().toDate() < new Date());
    }

    /**
    * Event handler for moving onto next event-step
    */
    nextBtnClick = async () => {
        this.setState({ isValidatingStep1: true });

        var isSubmitAllowed = await this.checkIfNextAllowed();
        if (isSubmitAllowed) {
            let modifiedState = { ...this.props.eventPageState };
            modifiedState.eventDetails = this.state.eventDetails;
            modifiedState.selectedCategory = this.state.selectedCategory;
            modifiedState.selectedEvent = this.state.selectedEventType;
            modifiedState.categories = this.state.categories;

            this.props.navigateToPage(2, modifiedState);
        }
    };

    /**
    * Event handler on selecting start date
    */
    setStartDate = (date: Date) => {
        let eventDetails = this.state.eventDetails;
        eventDetails.startDate = date;
        eventDetails.endDate = date;
        this.setState({ eventDetails: eventDetails });
    }

    /**
    * Event handler on selecting end date
    */
    setEndDate = (date: Date) => {
        let eventDetails = this.state.eventDetails;
        eventDetails.endDate = date;
        this.setState({ eventDetails: eventDetails });
    }

    /**
    * Event handler on fetching the validation message for valid name
    */
    getNameError = () => {
        if (!this.state.isNameValid) {
            return (<Text data-testid="event_name_req_error" content={this.localize("required")} error />);
        }
        else if (this.state.eventDetails.name.length > Resources.eventNameMaxLength) {
            return (<Text data-testid="event_name_max_error" content={this.localize("eventNameMaxCharError")} error />);
        }
        else if (this.state.isEventNameExisting) {
            return (<Text content={this.localize("eventNameAlreadyExists")} error />);
        }
        return (<></>);
    }

    /**
    * Event handler on fecthing the validation message for valid event team's meeting link
    */
    getMeetingLinkError = () => {
        if (!this.state.isMeetingLinkValid) {
            return (<Text data-testid="event_link_req_error" content={this.localize("validUrlErrorMessage")} error />);
        }

        return (<></>);
    }

    /**
    * Event handler on fetching the validation message for valid description
    */
    getDescriptionError = () => {
        if (!this.state.isDescriptionValid) {
            return (<Text data-testid="event_desc_req_error" content={this.localize("required")} error />);
        }
        if (this.state.eventDetails.description.length > Resources.eventDescriptionMaxLength) {
            return (<Text data-testid="event_desc_max_error" content={this.localize("eventDescriptionMaxCharError")} error />);
        }
        return (<></>);
    }

    /**
    * Event handler on fetching the validation message for valid venue
    */
    getVenueError = () => {
        if (!this.state.isVenueValid) {
            return (<Text data-testid="event_venue_req_error" content={this.localize("required")} error size="medium" />);
        }
        if (this.state.eventDetails.venue.length > Resources.eventVenueMaxLength) {
            return (<Text data-testid="event_venue_max_error" content={this.localize("eventVenueMaxCharError")} error />);
        }
        return (<></>);
    }

    /**
    * Event handler on fetching the validation message for valid event type
    */
    getEventTypeError = () => {
        if (!this.state.isEventTypeValid) {
            return (<Text data-testid="event_type_error" content={this.localize("required")} error />);
        }
        else {
            return (<Text content={""} error size="medium" />);
        }
    }

    /**
    * Event handler on fetching the validation message for valid event category
    */
    getEventCategoryError = () => {
        if (!this.state.isEventCategoryValid) {
            return (<Text data-testid="event_category_error" content={this.localize("required")} error />);
        }
        else {
            return (<Text content={""} error size="medium" />);
        }
    }

    /**
    * Event handler on fetching the validation message for valid image
    */
    getPhotoError = () => {
        if (!this.state.isPhotoValid) {
            return (<Text data-testid="event_photo_error" content={this.localize("required")} error />);
        }
        else if (!this.state.isPhotoDimentionsValid) {
            return (<Text content={this.localize("invalidImageDimensions")} error />);
        }
        else {
            return (<Text content={""} error size="medium" />);
        }
    }

    /**
    * Event handler on fetching the validation message for valid mnaximum no of participants
    */
    getMaximumNoofParticipantsError = () => {
        if (!this.state.isMaxNoofParticipantsValid) {
            return (<Text data-testid="event_max_req_error" content={this.localize("required")} error />);
        }
        return (<></>);
    }

    /**
    * Event handler on name change
    */
    onEventNameChange = (eventName: string) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, name: eventName },
            isNameValid: true,
            isEventNameExisting: false
        }));
    }

    /**
    * Event handler on description change
    */
    onEventDescriptionChange = (eventDescription: string) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, description: eventDescription },
            isDescriptionValid: true
        }));
    }

    /**
    * Event handler on venue change
    */
    onEventVenueChange = (eventVenue: string) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, venue: eventVenue },
            isVenueValid: true
        }));
    }

    /**
    * Event handler on live event url change
    */
    onLiveEventUrlChange = (eventUrl: string) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, meetingLink: eventUrl },
            isMeetingLinkValid: true
        }));
    }

    /**
    * Event handler on maximum number of participants change
    */
    onMaxNoOfParticipantsChange = (eventMaxNoofParticipants: string) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, maximumNumberOfParticipants: parseInt(eventMaxNoofParticipants) },
            isMaxNoofParticipantsValid: true
        }));
    }

    /**
    * Event handler on start time change
    */
    onStartTimeChange = (hours: number, min: number) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, startTime: new Date(new Date().setHours(hours, min)), endTime: this.state.eventDetails.endTime === undefined ? new Date(new Date().setHours(hours, min + 15)) : this.state.eventDetails.endTime }
        }));
    }

    /**
    * Event handler on end time change
    */
    onEndTimeChange = (hours: number, min: number) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, endTime: new Date(new Date().setHours(hours, min)) }
        }));
    }

    /**
    * Function calling a click event on a hidden file input
    */
    handleUploadClick = (event: any) => {
        this.inputReference.current.click();
    };

    /**
     * The event handler called when the selected color gets changed
     * @param id The color Id
     * @param color The color code
     */
    onColorChange = (id?: string | undefined, color?: string | undefined) => {
        this.setState((prevState: ICreateEventsStep1State) => ({
            eventDetails: { ...prevState.eventDetails, selectedColor: color, photo: "" }, inputKey: this.state.inputKey + 1, isPhotoDimentionsValid: true, isPhotoValid: true
        }));
    }

    /**
    * Function called for uploading image on azure
    */
    uploadImage = async (formData: FormData) => {
        this.setState({ imageUploadLoader: true });
        let response = await uploadEventImage(formData, this.teamId);

        if (response && response.status === ResponseStatus.OK && response.data) {
            this.setState((prevState: ICreateEventsStep1State) => ({
                eventDetails: { ...prevState.eventDetails, photo: response.data, selectedColor: "" },
                imageUploadLoader: false
            }));
        }
        else {
            this.setState({ imageUploadLoader: false });
        }
    }

    /**
    * Event Handler for image change
    */
    handleChange = (event: any) => {
        const fileUploaded = event.target.files[0];
        if (fileUploaded) {
            let isValid = true;
            let img = new Image
            img.src = window.URL.createObjectURL(fileUploaded)
            img.onload = () => {
                if (img.width < 850 || img.height < 290 || img.width > 900 || img.height > 310) {
                    isValid = false;
                } else {
                    const formData = new FormData();
                    formData.append(
                        "fileInfo",
                        fileUploaded,
                        fileUploaded.name
                    );
                    this.uploadImage(formData);
                }

                this.setState({ isPhotoDimentionsValid: isValid });
            }
        }
    };

    /** Validate whether the event name already exists */
    checkEventName = async () => {
        let response = await searchEventAsync(this.state.eventDetails.name);
        if (response && response.status === ResponseStatus.OK && response.data) {
            let isEventAlreadyExists = response.data.some((event: IEvent) => event.name.toLowerCase() === this.state.eventDetails.name.trim().toLowerCase());

            return isEventAlreadyExists;
        }
        else {
            return false;
        }
    }

    /**
    * Event handler for saving event as a draft
    */
    saveEventAsDraft = async () => {
        let result = this.checkIfSaveAsDraftAllowed();
        if (result) {
            this.setState({ isLoading: true });
            let modifiedState = { ...this.props.eventPageState };
            modifiedState.eventDetails = this.state.eventDetails;
            modifiedState.selectedCategory = this.state.selectedCategory;
            modifiedState.selectedEvent = this.state.selectedEventType;
            modifiedState.categories = this.state.categories;

            let result = await saveEventAsDraftAsync(modifiedState, this.teamId);
            if (result) {
                this.props.microsoftTeams.tasks.submitTask({ isSuccess: true, isDraft: true });
            }
            else {
                this.setState({ isLoading: false });
            }
        }
    }

    /** Renders a component */
    render() {
        const onEventTypeChange = {
            onAdd: (item: any) => {
                this.setState((prevState: ICreateEventsStep1State) => ({
                    eventDetails: { ...prevState.eventDetails, type: item.key },
                    isEventTypeValid: true,
                    selectedEventType: item
                }));
                return "";
            }
        }
        const onEventCategoryChange = (item: any) => {
            this.setState((prevState: ICreateEventsStep1State) => ({
                eventDetails: { ...prevState.eventDetails, categoryId: item.key },
                isEventCategoryValid: true,
                selectedCategory: item
            }));
        }

        const categoriesList = new Array<ISelectedDropdownItem>();
        for (var i = 0; i < this.state.categories.length; i++) {
            categoriesList.push({ key: this.state.categories[i].id.toString(), header: this.state.categories[i].name })
        }

        let minDate = new Date();
        minDate.setDate(minDate.getDate() + 1);

        return (
            <>
                <div className="page-content">
                    <Flex gap="gap.smaller">
                        <Text size="large" content={this.localize("eventDetailsStep1")} />
                    </Flex>
                    <Flex gap="gap.smaller" className="margin-top" vAlign="center">
                        <Text className="form-label margin-right" content={this.localize("eventPhotoStep1")} /><ExclamationCircleIcon title={this.localize("imageInfoIcon")} size="small" />
                        <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                            {this.getPhotoError()}
                        </Flex.Item>
                    </Flex>
                    {this.state.eventDetails.photo && <Flex gap="gap.smaller" className="input-label-margin-between" vAlign="end">
                        <FluentImage className="event-image-color" fluid src={this.state.eventDetails.photo} />
                    </Flex>}
                    {this.state.eventDetails.selectedColor && <Flex gap="gap.smaller" className="input-label-margin-between" vAlign="end">
                        <div className="event-image-color" style={{ backgroundColor: this.state.eventDetails.selectedColor }}>
                            <Flex className="event-image-color" hAlign="center" vAlign="center">
                                <Text className="event-color-text" size="large" weight="semibold" content={this.state.eventDetails.name} />
                            </Flex>
                        </div>
                    </Flex>}
                    <Flex gap="gap.smaller" className="input-label-margin-between" vAlign="center" >
                        <Flex.Item>
                            <>
                                <Button disabled={this.state.imageUploadLoader} loading={this.state.imageUploadLoader} onClick={this.handleUploadClick} size="medium" icon={<FilesUploadIcon className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}/>} content={this.state.eventDetails.photo && this.state.eventDetails.photo.length ? this.localize("changePhotoStep1") : this.localize("uploadPhotoStep1")} iconPosition="before" />
                                <input
                                    type="file"
                                    ref={this.inputReference}
                                    onChange={this.handleChange}
                                    style={{ display: 'none' }}
                                    key={this.state.inputKey}
                                />
                                <Text className={this.props.dir === LanguageDirection.Rtl ? "form-label rtl-left-margin-small" : "form-label"} content={this.localize("orLabelForImageOrColor")} />
                            </>
                        </Flex.Item>
                        <Flex.Item>
                            <div>
                                <ColorPicker onColorChange={this.onColorChange} selectedColor={this.state.eventDetails.selectedColor} />
                            </div>
                        </Flex.Item>
                    </Flex>
                    <Flex className="margin-top" gap="gap.smaller">
                        <Text className="form-label" content={this.localize("eventNameStep1")} />
                        <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                            {this.getNameError()}
                        </Flex.Item>
                    </Flex>
                    <Flex gap="gap.smaller" className="input-label-margin-between">
                        <Input data-testid="event_name_input" maxLength={Resources.eventNameMaxLength} fluid placeholder={this.localize("eventNamePlaceholderStep1")} value={this.state.eventDetails.name} onChange={(event: any) => this.onEventNameChange(event.target.value)} />
                    </Flex>
                    <Flex gap="gap.smaller" className="margin-top">
                        <Flex.Item size="size.half">
                            <Flex>
                                <Text className="form-label" content={this.localize("eventTypeStep1")} />
                                <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                                    {this.getEventTypeError()}
                                </Flex.Item>
                            </Flex>
                        </Flex.Item>
                        <Flex.Item size="size.half">
                            <Flex>
                                <Text className="form-label" content={this.localize("category")} />
                                <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                                    {this.getEventCategoryError()}
                                </Flex.Item>
                            </Flex>
                        </Flex.Item>

                    </Flex>
                    <Flex gap="gap.smaller" className="input-label-margin-between">
                        <Flex.Item size="size.half">
                            <Dropdown
                                className={this.props.dir === LanguageDirection.Rtl ? "dropdown-flex-half rtl-left-margin-small" : "dropdown-flex-half"}
                                fluid
                                items={this.state.eventTypes.map((value: IConstantDropdownItem) => { return { key: value.id, header: value.name } })}
                                value={this.state.selectedEventType}
                                placeholder={this.localize("selectEventTypePlaceholder")}
                                getA11ySelectionMessage={onEventTypeChange}
                                data-testid="event_type_dropdown"
                            />
                        </Flex.Item>
                        <Flex.Item size="size.half">
                            <Dropdown
                                className="dropdown-flex-half"
                                fluid
                                onChange={(event, data) => { onEventCategoryChange(data.value) }}
                                value={this.state.selectedCategory}
                                items={categoriesList}
                                placeholder={this.localize("selectCategoryPlaceholder")}
                                data-testid="event_category_dropdown"
                            />
                        </Flex.Item>
                    </Flex>
                    {
                        this.state.eventDetails.type === EventType.LiveEvent &&
                        <>
                            <Flex className="margin-top" gap="gap.smaller">
                                <Text className="form-label" content={this.localize("liveEventUrlStep1")} />
                                <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                                    {this.getMeetingLinkError()}
                                </Flex.Item>
                            </Flex>
                            <Flex gap="gap.smaller" className="input-label-margin-between">
                                <Input fluid value={this.state.eventDetails.meetingLink} placeholder={this.localize("liveEventUrlPlaceholder")}
                                    onChange={(event: any) => this.onLiveEventUrlChange(event.target.value)} data-testid="event_link_input" />
                            </Flex>
                        </>
                    }
                    {
                        this.state.eventDetails.type === EventType.InPerson && <>
                            <Flex className="margin-top" gap="gap.smaller">
                                <Text className="form-label" content={this.localize("venue")} />
                                <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                                    {this.getVenueError()}
                                </Flex.Item>
                            </Flex>
                            <Flex gap="gap.smaller" className="input-label-margin-between">
                                <Input
                                    maxLength={Resources.eventVenueMaxLength}
                                    value={this.state.eventDetails.venue}
                                    onChange={(event: any) => this.onEventVenueChange(event.target.value)}
                                    fluid
                                    placeholder={this.localize("venuePlaceholder")}
                                    data-testid="event_venue_input"
                                />
                            </Flex>
                        </>
                    }
                    <Flex gap="gap.smaller" className="margin-top">
                        <Text className="form-label" content={this.localize("eventDescriptionStep1")} />
                        <Flex.Item push grow={this.props.dir === LanguageDirection.Rtl}>
                            {this.getDescriptionError()}
                        </Flex.Item>
                    </Flex>
                    <Flex gap="gap.smaller" className="input-label-margin-between">
                        <TextArea
                            maxLength={Resources.eventDescriptionMaxLength}
                            fluid
                            placeholder={this.localize("eventDescriptionPlaceholder")}
                            value={this.state.eventDetails.description}
                            onChange={(event: any) => this.onEventDescriptionChange(event.target.value)}
                            data-testid="event_description_input"
                        />
                    </Flex>
                    <Flex gap="gap.smaller" className="margin-top">
                        <Text title={this.timeZone} truncated className="form-label" content={`${this.localize("yourTimeZone")}: ${this.timeZone}`} />
                    </Flex>
                    {
                        this.state.screenWidth >= 600 &&
                        <>
                            <Flex gap="gap.smaller" className="margin-top">
                                <Flex.Item size="size.half">
                                    <div>
                                        <Text className="form-label" align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={`${this.localize("startDateStep1")}`} />
                                        <Flex className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}>
                                            <StartDate
                                                screenWidth={this.state.screenWidth}
                                                theme={this.state.theme}
                                                selectedDate={this.state.eventDetails.startDate}
                                                minDate={minDate}
                                                onDateSelect={this.setStartDate}
                                                disableSelectionForPastDate={!this.checkEventAlreadyStarted()}
                                            />
                                        </Flex>
                                    </div>
                                </Flex.Item>
                                <Flex.Item size="size.half">
                                    <div>
                                        <Text className="form-label" align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={`${this.localize("endDateStep1")}`} />
                                        <StartDate
                                            screenWidth={this.state.screenWidth}
                                            theme={this.state.theme}
                                            minDate={this.state.eventDetails.endDate}
                                            selectedDate={this.state.eventDetails.endDate}
                                            onDateSelect={this.setEndDate}
                                            disableSelectionForPastDate={!this.checkEventAlreadyStarted()}
                                        />
                                    </div>
                                </Flex.Item>
                            </Flex>
                        </>
                    }
                    {
                        this.state.screenWidth < 600 &&
                        <React.Fragment>
                            <Flex gap="gap.smaller" className="margin-top">
                                <Text className="form-label" content={this.localize("startDateStep1")} />
                            </Flex>
                            <Flex gap="gap.smaller" className="input-label-margin-between">
                                <StartDate
                                    screenWidth={this.state.screenWidth}
                                    theme={this.state.theme}
                                    selectedDate={this.state.eventDetails.startDate}
                                    minDate={minDate}
                                    onDateSelect={this.setStartDate}
                                    disableSelectionForPastDate={!this.checkEventAlreadyStarted()}
                                />
                            </Flex>
                            <Flex gap="gap.smaller" className="margin-top">
                                <Text className="form-label" content={this.localize("endDateStep1")} />
                            </Flex>
                            <Flex gap="gap.smaller" className="input-label-margin-between">
                                <StartDate
                                    screenWidth={this.state.screenWidth}
                                    theme={this.state.theme}
                                    minDate={this.state.eventDetails.startDate}
                                    selectedDate={this.state.eventDetails.endDate}
                                    onDateSelect={this.setEndDate}
                                    disableSelectionForPastDate={!this.checkEventAlreadyStarted()}
                                />
                            </Flex>
                        </React.Fragment>
                    }

                    <Flex gap="gap.smaller" className="margin-top">
                        <Flex.Item size="size.half">
                            <Flex gap="gap.smaller">
                                <Flex.Item size="size.half">
                                    <Text className="form-label" align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.localize("startTimeStep1")} />
                                </Flex.Item>
                                <Flex.Item size="size.half">
                                    <Text className="form-label" align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.localize("endTimeStep1")} />
                                </Flex.Item>
                            </Flex>
                        </Flex.Item>
                        <Flex.Item size="size.half">
                            <Flex>
                                <Text className="form-label" align={this.props.dir === LanguageDirection.Rtl ? "end" : "start"} content={this.localize("maxParticipantsStep1")} />
                                <Flex.Item push={this.props.dir === LanguageDirection.Ltr} grow={this.props.dir === LanguageDirection.Rtl}>
                                    {this.getMaximumNoofParticipantsError()}
                                </Flex.Item>
                            </Flex>
                        </Flex.Item>
                    </Flex>
                    <Flex gap="gap.smaller" className="input-label-margin-between">
                        <Flex.Item size="size.half" className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}>
                            <Flex gap="gap.smaller">
                                <Flex.Item size="size.half">
                                    <TimePicker
                                        hours={this.state.eventDetails.startTime?.getHours()}
                                        minutes={this.state.eventDetails.startTime?.getMinutes()}
                                        isDisabled={!this.checkEventAlreadyStarted()}
                                        onPickerClose={this.onStartTimeChange}
                                        dir={this.props.dir}
                                    />
                                </Flex.Item>
                                <Flex.Item size="size.half">
                                    <TimePicker
                                        hours={this.state.eventDetails.endTime?.getHours()}
                                        minutes={this.state.eventDetails.endTime?.getMinutes()}
                                        onPickerClose={this.onEndTimeChange}
                                        minHours={this.state.eventDetails.startTime?.getHours()!}
                                        minMinutes={undefined}
                                        isDisabled={!this.checkEventAlreadyStarted()}
                                        dir={this.props.dir}
                                    />
                                </Flex.Item>
                            </Flex>
                        </Flex.Item>
                        <Flex.Item size="size.half" className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}>
                            <Input type="number" min={1} fluid value={this.state.eventDetails.maximumNumberOfParticipants.toString()} onChange={(event: any) => this.onMaxNoOfParticipantsChange(event.target.value)} data-testid="event_maxParticipants_input" />
                        </Flex.Item>
                    </Flex>
                </div>
                {this.props.dir === LanguageDirection.Ltr && <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    {!this.state.isTimeValid && <Text error content={this.localize("startAndEndTimeError")} />}
                    <Flex.Item push>
                        <Text weight="bold" content={this.localize("step1Of3")} />
                    </Flex.Item>
                    {(!this.props.eventPageState.isEdit || (this.props.eventPageState.isEdit && this.props.eventPageState.isDraft)) && <Button disabled={this.state.isLoading || this.state.isValidatingStep1} loading={this.state.isLoading} onClick={this.saveEventAsDraft} content={this.localize("saveAsDraft")} secondary data-testid="event_save_button" />}
                    <Button data-testid="event_next_button" content={this.localize("nextButton")} loading={this.state.isValidatingStep1} disabled={this.state.isLoading || this.state.isValidatingStep1} primary onClick={this.nextBtnClick} />
                </Flex>}

                {this.props.dir === LanguageDirection.Rtl && <Flex gap="gap.smaller" className="button-footer" vAlign="center">
                    <Flex.Item push>
                        <Text error content={!this.state.isTimeValid ? this.localize("startAndEndTimeError") : ""} />
                    </Flex.Item>
                    <Text className="rtl-left-margin-small" weight="bold" content={this.localize("step1Of3")} />
                    {(!this.props.eventPageState.isEdit || (this.props.eventPageState.isEdit && this.props.eventPageState.isDraft)) && <Button className="rtl-left-margin-small" disabled={this.state.isLoading || this.state.isValidatingStep1} loading={this.state.isLoading} onClick={this.saveEventAsDraft} content={this.localize("saveAsDraft")} secondary data-testid="event_save_button" />}
                    <Button data-testid="event_next_button" content={this.localize("nextButton")} loading={this.state.isValidatingStep1} disabled={this.state.isLoading || this.state.isValidatingStep1} primary onClick={this.nextBtnClick} />
                </Flex>}
            </>
        );
    }
}

export default withTranslation()(withContext(CreateEventStep1));