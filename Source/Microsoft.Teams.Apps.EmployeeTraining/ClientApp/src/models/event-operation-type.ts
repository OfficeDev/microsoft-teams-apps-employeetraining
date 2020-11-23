// <copyright file="event-operation-type.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

/** Indicates the event operation */
export enum EventOperationType {
    /** No event operation */
    None,

    /** The operation in task module to close event registrations */
    CloseRegistration,

    /** The operation in task module to cancel an event to occur */
    CancelEvent,

    /** The operation in task module to register for an event */
    Register,

    /** The operation in task module to cancel event registration */
    Remove
}