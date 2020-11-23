// <copyright file="localized-constants.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { EventAudience } from "../models/event-audience";
import { EventType } from "../models/event-type";
import { IConstantDropdownItem } from "../constants/resources";
import Resources from "../constants/resources";
import { TFunction } from "i18next";
import { IPostType } from "../models/IPostType";
import { SortBy } from "../models/sort-by";

/**
* Get localized audience types.
* @param localize i18n TFunction received from props.
*/
export const getLocalizedAudienceTypes = (localize: TFunction): Array<IConstantDropdownItem> => {
    return Resources.audienceType.map((value: IConstantDropdownItem) => {
        switch (value.id) {
            case EventAudience.Public:
                value.name = localize("publicAudience");
                return value;

            case EventAudience.Private:
                value.name = localize("privateAudience");
                return value;

            default:
                return value;
        }
    });
}

/**
* Get localized event types.
* @param localize i18n TFunction received from props.
*/
export const getLocalizedEventTypes = (localize: TFunction): Array<IConstantDropdownItem> => {
    return Resources.eventType.map((value: IConstantDropdownItem) => {
        switch (value.id) {
            case EventType.InPerson:
                value.name = localize("inPersonEvent");
                return value;

            case EventType.Teams:
                value.name = localize("teamsEvent");
                return value;

            case EventType.LiveEvent:
                value.name = localize("liveEvent");
                return value;

            default:
                return value;
        }
    });
}

/**
* Get localized sort by filters.
* @param localize i18n TFunction received from props.
*/
export const getLocalizedSortBy = (localize: TFunction): Array<IPostType> => {
    return Resources.sortBy.map((value: IPostType) => {
        switch (value.id) {
            case SortBy.Recent:
                value.name = localize("sortByNewest");
                return value;

            case SortBy.Popularity:
                value.name = localize("sortByPopularity");
                return value;

            default:
                return value;
        }
    });
}