// <copyright file="create-event-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
import { ResponseStatus } from "../../constants/constants";
import { IEvent } from "../../models/IEvent";
import { ICategory } from "../../models/category";

/** The base URL for API */
const baseURL = window.location.origin + "/api";

/**
 * Upload image photo
 * @param formData Form data containing selected image
 * @param teamId The LnD team ID
 */
export const uploadEventImage = async (formData: FormData, teamId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
 * Save event as draft
 * @param event Event details to be saved as draft
 * @param teamId The LnD team ID
 */
export const saveEventAsDraft = async (event: IEvent, teamId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
 * Update draft event
 * @param event Event details to be updated as draft
 * @param teamId The LnD team ID
 */
export const updateEventAsDraft = async (event: IEvent, teamId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
 * Create event and add to calendar
 * @param event Event details to be saved
 * @param teamId The LnD team ID
 */
export const createNewEvent = async (event: IEvent, teamId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
 * Update event details
 * @param event Event details to be updated
 * @param teamId The LnD team ID
 */
export const updateEvent = async (event: IEvent, teamId: string) => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}

/**
 * Gets event categories
 * @param teamId The LnD team ID
 */
export const getEventCategoriesAsync = async () => {
    let categories: Array<ICategory> = [
        { categoryId: "category1", description: "description1", isInUse: false, isSelected: false, name: "categoryname1", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() },
        { categoryId: "category2", description: "description2", isInUse: false, isSelected: false, name: "categoryname2", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() },
        { categoryId: "category3", description: "description3", isInUse: true, isSelected: false, name: "categoryname3", createdBy: "test", createdOn: new Date(), updatedBy: "test", updatedOn: new Date() }
    ]
    return Promise.resolve({
        data: categories,
        status: ResponseStatus.OK
    });
}

/**
 * Check for event with same name
 * @param eventName User entered event name
 */
export const searchEventAsync = async (eventName: string) => {
    console.log("AAAAAAAAAAAA");
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}