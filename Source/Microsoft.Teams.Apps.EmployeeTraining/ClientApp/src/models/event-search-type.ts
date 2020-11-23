// <copyright file="event-search-type.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export enum EventSearchType {
    /** Represents operation type to get all public and private events for user */
    AllPublicPrivateEventsForUser,

    /** Represents operation type to get mandatory events for user */
    MandatoryEventsForUser,

    /** Represents operation type to get draft events for team */
    DraftEventsForTeam,

    /** Represents operation type to get active events for team */
    ActiveEventsForTeam,

    /** Represents operation type to get completed events for team */
    CompletedEventsForTeam,

    /** Represents operation type to get registered events for a user */
    RegisteredEventsForUser,

    /** Represents operation type to get completed events for a user */
    CompletedEventsForUser = 7,
}