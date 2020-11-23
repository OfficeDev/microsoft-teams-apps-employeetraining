// <copyright file="sort-by.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

/** Represents 0 for recent and 1 for popular events */
export enum SortBy {
    /** Represents default sorting of event by most recent first */
    Recent,

    /** Represents sorting of events by most number of registered attendee for an event */
    Popularity,
}