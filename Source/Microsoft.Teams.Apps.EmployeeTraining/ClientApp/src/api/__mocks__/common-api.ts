// <copyright file="create-event-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
import { ResponseStatus } from "../../constants/constants";
import TestData from "../test-data/test-data";

/** The base URL for API */
const baseURL = window.location.origin + "/api";

/**
 * Gets event details
 * @param eventId The event ID of which details need to be retrieved
 * @param teamId The LnD team ID
 */
export const getEventAsync = async (eventId: string, teamId: string) => {
    return Promise.resolve({
        data: TestData.testEventDetails,
        status: ResponseStatus.OK
    });
}