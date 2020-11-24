// <copyright file="user-events-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ResponseStatus } from "../../constants/constants";
import { IEvent } from "../../models/IEvent";
import { EventStatus } from "../../models/event-status";

/** The base URL for API */
const baseURL = window.location.origin + "/api";

/**
 * Register to an event
 * @param eventId The event Id in which registration need to be done
 */
export const getEventsAsync = async (
    searchString: string, pageCount: number, eventSearchType: number, createdByFilter: string, categoryFilter: string, sortByFilter: number) => {
    let events: Array<IEvent> = [
        {
            eventId: "1",
            teamId: "19:a1cce87adc81404c8b3b61276a7dfe0b@thread.tacv2",
            name: "Draft Event 1",
            description: "Draft Event 1",
            startDate: new Date("2020-09-30T08:30:13Z"),
            venue: "",
            categoryId: "category1",
            registeredAttendeesCount: 16,
            maximumNumberOfParticipants: 20,
            status: EventStatus.Draft,
            audience: 2,
            startTime: new Date("2020-09-28T08:30:13.422Z"),
            endTime: new Date("2020-09-28T09:30:15.943Z"),
            graphEventId: "AAMkADBhZWQyNzAwLWMzYTMtNGIyZi04MzMwLWE1ZGRhNzAwNzUwNABGAAAAAAAZ6TOP56tHR7uCxRRJ2P4DBwC56vz5BTDmQpfmcre0NyZhAAAAAAENAAC56vz5BTDmQpfmcre0NyZhAACcfCrLAAA=",
            photo: "https://siddharthstorage.blob.core.windows.net/events-photos/91e5703b-139f-49fb-bcfb-b01ae1463c6b",
            numberOfOccurrences: 1,
            isAutoRegister: true,
            type: 2,
            meetingLink: "",
            createdOn: new Date("2020-09-28T06:53:46.235Z"),
            createdBy: "user2",
            isRegistrationClosed: true,
            isMandatoryForLoggedInUser: false,
            isLoggedInUserRegistered: false,
            endDate: new Date("2020-09-30T09:30:15Z"),
            categoryName: "",
            mandatoryAttendees: "user1;user2",
            optionalAttendees: "",
            registeredAttendees: "26",
            updatedOn: new Date("2020-09-29T07:55:00.585825Z"),
            selectedUserOrGroupListJSON: "[{\"displayName\":\"All Employees\",\"email\":\"Employees@M365x002616.OnMicrosoft.com\",\"id\":\"1\",\"isGroup\":true,\"isMandatory\":true}]",
            autoRegisteredAttendees: "user1;user2"
        },
        {
            eventId: "2",
            teamId: "19:a1cce87adc81404c8b3b61276a7dfe0b@thread.tacv2",
            name: "Draft Event 2",
            description: "Draft Event 2",
            startDate: new Date("2020-09-30T08:30:13Z"),
            venue: "",
            categoryId: "category2",
            registeredAttendeesCount: 26,
            maximumNumberOfParticipants: 20,
            status: EventStatus.Draft,
            audience: 2,
            startTime: new Date("2020-09-28T08:30:13.422Z"),
            endTime: new Date("2020-09-28T09:30:15.943Z"),
            graphEventId: "AAMkADBhZWQyNzAwLWMzYTMtNGIyZi04MzMwLWE1ZGRhNzAwNzUwNABGAAAAAAAZ6TOP56tHR7uCxRRJ2P4DBwC56vz5BTDmQpfmcre0NyZhAAAAAAENAAC56vz5BTDmQpfmcre0NyZhAACcfCrLAAA=",
            photo: "https://siddharthstorage.blob.core.windows.net/events-photos/91e5703b-139f-49fb-bcfb-b01ae1463c6b",
            numberOfOccurrences: 1,
            isAutoRegister: true,
            type: 2,
            meetingLink: "",
            createdOn: new Date("2020-09-28T06:53:46.235Z"),
            createdBy: "user3",
            isRegistrationClosed: true,
            isMandatoryForLoggedInUser: false,
            isLoggedInUserRegistered: false,
            endDate: new Date("2020-09-30T09:30:15Z"),
            categoryName: "",
            mandatoryAttendees: "user4",
            optionalAttendees: "",
            registeredAttendees: "26",
            updatedOn: new Date("2020-09-29T07:55:00.585825Z"),
            selectedUserOrGroupListJSON: "[{\"displayName\":\"All Employees\",\"email\":\"Employees@M365x002616.OnMicrosoft.com\",\"id\":\"5a501b90-9fae-4e3c-b7bb-3f14a9e6fb84\",\"isGroup\":true,\"isMandatory\":true}]",
            autoRegisteredAttendees: "1"
        },
        {
            eventId: "3",
            teamId: "19:a1cce87adc81404c8b3b61276a7dfe0b@thread.tacv2",
            name: "Active Event 1",
            description: "Active Event 1",
            startDate: new Date("2020-10-01T08:30:13Z"),
            venue: "",
            categoryId: "category1",
            registeredAttendeesCount: 26,
            maximumNumberOfParticipants: 20,
            status: EventStatus.Active,
            audience: 2,
            startTime: new Date("2020-10-01T08:30:13.422Z"),
            endTime: new Date("2020-10-03T09:30:15.943Z"),
            graphEventId: "AAMkADBhZWQyNzAwLWMzYTMtNGIyZi04MzMwLWE1ZGRhNzAwNzUwNABGAAAAAAAZ6TOP56tHR7uCxRRJ2P4DBwC56vz5BTDmQpfmcre0NyZhAAAAAAENAAC56vz5BTDmQpfmcre0NyZhAACcfCrLAAA=",
            photo: "https://siddharthstorage.blob.core.windows.net/events-photos/91e5703b-139f-49fb-bcfb-b01ae1463c6b",
            numberOfOccurrences: 1,
            isAutoRegister: true,
            type: 1,
            meetingLink: "",
            createdOn: new Date("2020-09-28T06:53:46.235Z"),
            createdBy: "user2",
            isRegistrationClosed: true,
            isMandatoryForLoggedInUser: false,
            isLoggedInUserRegistered: false,
            endDate: new Date("2020-10-03T09:30:15Z"),
            categoryName: "",
            mandatoryAttendees: "user3",
            optionalAttendees: "",
            registeredAttendees: "6",
            updatedOn: new Date("2020-09-29T07:55:00.585825Z"),
            selectedUserOrGroupListJSON: "[{\"displayName\":\"All Employees\",\"email\":\"Employees@M365x002616.OnMicrosoft.com\",\"id\":\"5a501b90-9fae-4e3c-b7bb-3f14a9e6fb84\",\"isGroup\":true,\"isMandatory\":true}]",
            autoRegisteredAttendees: "user4;user1"
        },
        {
            eventId: "4",
            teamId: "19:a1cce87adc81404c8b3b61276a7dfe0b@thread.tacv2",
            name: "Completed Event 1",
            description: "Completed Event 1",
            startDate: new Date("2020-09-30T08:30:13Z"),
            venue: "",
            categoryId: "category3",
            registeredAttendeesCount: 46,
            maximumNumberOfParticipants: 50,
            status: EventStatus.Completed,
            audience: 2,
            startTime: new Date("2020-09-28T08:30:13.422Z"),
            endTime: new Date("2020-09-28T09:30:15.943Z"),
            graphEventId: "AAMkADBhZWQyNzAwLWMzYTMtNGIyZi04MzMwLWE1ZGRhNzAwNzUwNABGAAAAAAAZ6TOP56tHR7uCxRRJ2P4DBwC56vz5BTDmQpfmcre0NyZhAAAAAAENAAC56vz5BTDmQpfmcre0NyZhAACcfCrLAAA=",
            photo: "https://siddharthstorage.blob.core.windows.net/events-photos/91e5703b-139f-49fb-bcfb-b01ae1463c6b",
            numberOfOccurrences: 1,
            isAutoRegister: true,
            type: 3,
            meetingLink: "",
            createdOn: new Date("2020-09-28T06:53:46.235Z"),
            createdBy: "user2",
            isRegistrationClosed: true,
            isMandatoryForLoggedInUser: false,
            isLoggedInUserRegistered: false,
            endDate: new Date("2020-09-30T09:30:15Z"),
            categoryName: "",
            mandatoryAttendees: "",
            optionalAttendees: "",
            registeredAttendees: "26",
            updatedOn: new Date("2020-09-29T07:55:00.585825Z"),
            selectedUserOrGroupListJSON: "[{\"displayName\":\"All Employees\",\"email\":\"Employees@M365x002616.OnMicrosoft.com\",\"id\":\"5a501b90-9fae-4e3c-b7bb-3f14a9e6fb84\",\"isGroup\":true,\"isMandatory\":true}]",
            autoRegisteredAttendees: ""
        },
        {
            eventId: "5",
            teamId: "19:a1cce87adc81404c8b3b61276a7dfe0b@thread.tacv2",
            name: "Completed Event 2",
            description: "Completed Event 2",
            startDate: new Date("2020-09-30T08:30:13Z"),
            venue: "",
            categoryId: "category3",
            registeredAttendeesCount: 6,
            maximumNumberOfParticipants: 10,
            status: EventStatus.Completed,
            audience: 2,
            startTime: new Date("2020-09-28T08:30:13.422Z"),
            endTime: new Date("2020-09-28T09:30:15.943Z"),
            graphEventId: "AAMkADBhZWQyNzAwLWMzYTMtNGIyZi04MzMwLWE1ZGRhNzAwNzUwNABGAAAAAAAZ6TOP56tHR7uCxRRJ2P4DBwC56vz5BTDmQpfmcre0NyZhAAAAAAENAAC56vz5BTDmQpfmcre0NyZhAACcfCrLAAA=",
            photo: "https://siddharthstorage.blob.core.windows.net/events-photos/91e5703b-139f-49fb-bcfb-b01ae1463c6b",
            numberOfOccurrences: 1,
            isAutoRegister: true,
            type: 2,
            meetingLink: "",
            createdOn: new Date("2020-09-28T06:53:46.235Z"),
            createdBy: "user1",
            isRegistrationClosed: true,
            isMandatoryForLoggedInUser: false,
            isLoggedInUserRegistered: false,
            endDate: new Date("2020-09-30T09:30:15Z"),
            categoryName: "",
            mandatoryAttendees: "user4",
            optionalAttendees: "",
            registeredAttendees: "7",
            updatedOn: new Date("2020-09-29T07:55:00.585825Z"),
            selectedUserOrGroupListJSON: "[{\"displayName\":\"All Employees\",\"email\":\"Employees@M365x002616.OnMicrosoft.com\",\"id\":\"5a501b90-9fae-4e3c-b7bb-3f14a9e6fb84\",\"isGroup\":true,\"isMandatory\":true}]",
            autoRegisteredAttendees: "user4;user3"
        }
    ]
    let searchEvents: IEvent[] = events;
    if (searchString !== "") {
        console.log(searchString);
        return Promise.resolve({
            data: [events[0]],
            status: ResponseStatus.OK
        });
    }

    if (eventSearchType === 0) {
        return Promise.resolve({
            data: [],
            status: ResponseStatus.OK
        });
    }

    if (eventSearchType === 7) {
        return Promise.resolve({
            data: [],
            status: ResponseStatus.OK
        });
    }
    switch (sortByFilter) {
        case 0:
            searchEvents = events;
            break;
        case 1:
            searchEvents = [events[0], events[1]];
            break;
        default:
            break;
    }

    switch (categoryFilter) {
        case "category1":
            searchEvents = [events[0], events[1], events[2]]
            break;
        case "category2":
            searchEvents = [events[2]]
            break;
        case "category3":
            searchEvents = [events[2], events[3], events[1]]
            break;
        default:
            break;
    }

    switch (createdByFilter) {
        case "user1":
            searchEvents = [events[0], events[1], events[2], events[3]]
            break;
        case "user2":
            searchEvents = [events[0], events[2]]
            break;
        case "user3":
            searchEvents = [events[2], events[3], events[1]]
            break;
        default:
            break;
    }
    return Promise.resolve({
        data: searchEvents,
        status: ResponseStatus.OK
    });
}

/**
 * Registers user to an event
 * @param teamId The LnD team ID who created the event
 * @param eventId The event Id in which registration need to be done
 */
export const registerToEventAsync = (teamId: string, eventId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
 * Un-register user to an event
 * @param teamId The LnD team ID who created the event
 * @param eventId The event Id in which registration need to be cancelled
 */
export const removeEventAsync = (teamId: string, eventId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}