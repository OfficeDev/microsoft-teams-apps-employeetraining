// <copyright file="event-type.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export enum EventType {
    /** Indicates that the event type is not specified */
    None,

    /** Indicates that the event occurs in physical presence */
    InPerson = 1,

    /** Indicates that the event is a Microsoft Teams meeting */
    Teams = 2,

    /** Indicates that the event is a Microsoft Teams meeting */
    LiveEvent = 3,
}