// <copyright file="event-status.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

/** Contains the values for event status */
export enum EventStatus {
    /** Indicates that the event status is not specified */
    None,

    /** Indicates that the event is in draft */
    Draft,

    /** Indicates that the event is active */
    Active,

    /** Indicates that the event has been cancelled */
    Cancelled,

    /** Indicates that the event has been completed */
    Completed
}