// <copyright file="ICreateEventState.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { IEvent } from "./IEvent";
import { ISelectedUserGroup } from "./ISelectedUserGroup";
import { ISelectedDropdownItem } from "./ISelectedDropdownItem";
import { IConstantDropdownItem } from "../constants/resources";

export interface ICreateEventState {
    currentEventStep: number,
    eventDetails: IEvent,
    selectedCategory: ISelectedDropdownItem | undefined,
    selectedEvent: ISelectedDropdownItem | undefined,
    selectedAudience: ISelectedDropdownItem | undefined,
    selectedUserGroups: Array<ISelectedUserGroup>,
    categories: Array<IConstantDropdownItem>,
    isEdit: boolean,
    isDraft: boolean,
    isLoading: boolean,
    displayReadonly: boolean
}