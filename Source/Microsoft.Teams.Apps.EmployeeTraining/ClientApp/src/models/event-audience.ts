// <copyright file="event-audience.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export enum EventAudience {
    /** Indicates that the event audience is not specified */
    None,

    /** Indicates that the event is public */
    Public = 1,

    /** Indicates that the event is private */
    Private = 2,
}