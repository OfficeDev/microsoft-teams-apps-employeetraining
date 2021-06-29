import { createNewEvent, saveEventAsDraft, updateEvent, updateEventAsDraft } from "../api/create-event-api";
import { ResponseStatus } from "../constants/constants";
import { ISelectedUserGroup } from "../models/ISelectedUserGroup";
import { getGroupMembers } from "../api/user-group-api";
import { ICreateEventState } from "../models/ICreateEventState";
import moment from 'moment';

export const validateSelectedUsers = async (selectedUsersAndGroups: Array<ISelectedUserGroup>) => {
    let users = { mandatoryUsers: new Array<string>(), optionalUsers: new Array<string>() };
    let groups = { mandatoryUsers: new Array<string>(), optionalUsers: new Array<string>() };

    let filteredUsers = { mandatoryUsers: new Array<string>(), optionalUsers: new Array<string>() };

    for (let i = 0; i < selectedUsersAndGroups.length; i++) {
        let userOrGroup = selectedUsersAndGroups[i];
        if (userOrGroup.isGroup) {
            let response = await getGroupMembers(userOrGroup.id);
            if (response.status === ResponseStatus.OK) {
                let members = response.data.map((member) => { return member.id });
                if (userOrGroup.isMandatory) {
                    groups.mandatoryUsers.push(...members);
                }
                else {
                    groups.optionalUsers.push(...members);
                }
            }
        }
        else {
            if (userOrGroup.isMandatory) {
                users.mandatoryUsers.push(userOrGroup.id);
            }
            else {
                users.optionalUsers.push(userOrGroup.id);
            }
        }
    }

    // Remove duplicates
    users.mandatoryUsers = [...new Set(users.mandatoryUsers)];
    users.optionalUsers = [...new Set(users.optionalUsers)];
    groups.mandatoryUsers = [...new Set(groups.mandatoryUsers)];
    groups.optionalUsers = [...new Set(groups.optionalUsers)];

    // Remove users from optional array if present in both mandatory and optional array (for users).
    let filteredOptionalUsers = new Array<string>();
    for (let j = 0; j < users.optionalUsers.length; j++) {
        let result = users.mandatoryUsers.find((mandatoryUserId) => mandatoryUserId === users.optionalUsers[j]);
        if (!result) {
            filteredOptionalUsers.push(users.optionalUsers[j]);
        }
    }
    users.optionalUsers = filteredOptionalUsers;

    // Remove users from optional array if present in both mandatory and optional array (for users in group).
    let filteredOptionalUsersInGroup = new Array<string>();
    for (let k = 0; k < groups.optionalUsers.length; k++) {
        let result = groups.mandatoryUsers.find((mandatoryUserId) => mandatoryUserId === groups.optionalUsers[k]);
        if (!result) {
            filteredOptionalUsersInGroup.push(groups.optionalUsers[k]);
        }
    }

    groups.optionalUsers = filteredOptionalUsersInGroup;

    // Check if user from group is added again as single user.
    // If true then single user entity's mandatory/optional status will be considered and group's status will be neglected for that user.
    let filteredMandatoryUsersInGroup = new Array<string>();
    for (let l = 0; l < groups.mandatoryUsers.length; l++) {
        let result = users.optionalUsers.find((optionalUser) => optionalUser === groups.mandatoryUsers[l]);
        if (!result) {
            filteredMandatoryUsersInGroup.push(groups.mandatoryUsers[l]);
        }
    }
    groups.mandatoryUsers = filteredMandatoryUsersInGroup;

    filteredOptionalUsersInGroup = new Array<string>();
    for (let m = 0; m < groups.optionalUsers.length; m++) {
        let result = users.mandatoryUsers.find((mandatoryUser) => mandatoryUser === groups.optionalUsers[m]);
        if (!result) {
            filteredOptionalUsersInGroup.push(groups.optionalUsers[m]);
        }
    }
    groups.optionalUsers = filteredOptionalUsersInGroup;

    filteredUsers.mandatoryUsers.push(...users.mandatoryUsers);
    filteredUsers.mandatoryUsers.push(...groups.mandatoryUsers);
    filteredUsers.optionalUsers.push(...users.optionalUsers);
    filteredUsers.optionalUsers.push(...groups.optionalUsers);

    filteredUsers.mandatoryUsers = [...new Set(filteredUsers.mandatoryUsers)];
    filteredUsers.optionalUsers = [...new Set(filteredUsers.optionalUsers)];

    return { mandatoryUsers: filteredUsers.mandatoryUsers, optionalUsers: filteredUsers.optionalUsers };
}

export const createEvent = async (stepEventState: ICreateEventState, teamId: string) => {
    let eventDetails = { ...stepEventState.eventDetails };
    eventDetails.startDate = moment(eventDetails.startDate).startOf('day').add(eventDetails.startTime?.getHours(), 'hours').add(eventDetails.startTime?.getMinutes(), 'minutes').utc().toDate();
    eventDetails.endDate = moment(eventDetails.endDate).startOf('day').add(eventDetails.startTime?.getHours(), 'hours').add(eventDetails.startTime?.getMinutes(), 'minutes').utc().toDate();
    eventDetails.startTime = moment(eventDetails.startTime).utc().toDate();
    eventDetails.endTime = moment(eventDetails.endTime).utc().toDate();
    eventDetails.selectedUserOrGroupListJSON = JSON.stringify(stepEventState.selectedUserGroups);
    let response = await createNewEvent(eventDetails, teamId);

    if (response.status === ResponseStatus.OK) {
        return true;
    }
    else {
        return false;
    }
}

export const updateEventDetails = async (stepEventState: ICreateEventState, teamId: string) => {
    let eventDetails = { ...stepEventState.eventDetails };
    eventDetails.startDate = moment(eventDetails.startDate).utc().toDate();
    eventDetails.endDate = moment(eventDetails.endDate).utc().toDate();
    eventDetails.startTime = moment(eventDetails.startTime).utc().toDate();
    eventDetails.endTime = moment(eventDetails.endTime).utc().toDate();
    eventDetails.selectedUserOrGroupListJSON = JSON.stringify(stepEventState.selectedUserGroups);
    let response = await updateEvent(eventDetails, teamId);

    if (response.status === ResponseStatus.OK) {
        return true;
    }
    else {
        return false;
    }
}

export const saveEventAsDraftAsync = async (stepEventState: ICreateEventState, teamId: string) => {
    let eventDetails = stepEventState.eventDetails;
    // let users = await getUserIdString(stepEventState.selectedUserGroups);
    eventDetails.startDate = moment(eventDetails.startDate).utc().toDate();
    eventDetails.endDate = moment(eventDetails.endDate).utc().toDate();
    eventDetails.startTime = moment(eventDetails.startTime).utc().toDate();
    eventDetails.endTime = moment(eventDetails.endTime).utc().toDate();
    eventDetails.selectedUserOrGroupListJSON = JSON.stringify(stepEventState.selectedUserGroups);

    if (stepEventState.isDraft) {
        let response = await updateEventAsDraft(eventDetails, teamId);

        if (response.status === ResponseStatus.OK) {
            return true;
        }
        return false;
    }
    else {
        let response = await saveEventAsDraft(eventDetails, teamId);

        if (response.status === ResponseStatus.OK) {
            return true;
        }
        return false;
    }
}

/**
    * Format and renders event day and time as per local time.
    * @param startDate The start date of an event
    * @param startTime The start time of an event
    * @param endTime The end time of an event
    * @returns Returns formatted date and time. Ex. Tue, 10:00 - 11:00
    */
export const formatEventDayAndTimeToShort = (startDate: Date, startTime: Date, endTime: Date) => {
    let eventDay = moment.utc(startDate).local().format("ddd");
    let eventStartTime = moment.utc(startTime).local().format("HH:mm");
    let eventEndTime = moment.utc(endTime).local().format("HH:mm");

    return `${eventDay}, ${eventStartTime} - ${eventEndTime}`;
}