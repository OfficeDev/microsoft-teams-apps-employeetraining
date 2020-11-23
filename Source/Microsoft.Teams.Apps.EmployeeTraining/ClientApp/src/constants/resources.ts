/*
	<copyright file="resources.ts" company="Microsoft">
	Copyright (c) Microsoft. All rights reserved.
	</copyright>
*/

import { EventAudience } from "../models/event-audience";
import { EventType } from "../models/event-type";
import { IPostType } from "../models/IPostType";
import { SortBy } from "../models/sort-by";

export interface IConstantDropdownItem {
	name: string;
	id: number;
}

export interface ITimeZonesItem {
	displayName: string;
	id: string;
}

export default class Resources {
	public static readonly dark: string = "dark";
	public static readonly contrast: string = "contrast";
	public static readonly eventNameMaxLength: number = 100;
	public static readonly eventDescriptionMaxLength: number = 1000;
	public static readonly eventVenueMaxLength: number = 200;
	public static readonly userEventsMobileFilteredCategoriesLocalStorageKey: string = "user-events-filtered-categories";
	public static readonly userEventsMobileFilteredUsersLocalStorageKey: string = "user-events-filtered-users";
	public static readonly userEventsMobileSortByFilterLocalStorageKey: string = "user-events-sortby";
	public static readonly validUrlRegExp: RegExp = /^http(s)?:\/\/(www\.)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$/;

	/** Color codes used while creating an event */
	public static readonly colorCells = [
		{ id: 'a', label: 'Wild blue yonder', color: '#A4A8CB' },
		{ id: 'b', label: 'Jasmine', color: '#FFDE85' },
		{ id: 'c', label: 'Sky blue', color: '#A0EAF8' },
		{ id: 'd', label: 'Nadeshiko pink', color: '#F1A7B9' },
		{ id: 'e', label: 'Lavender blue', color: '#E3D7FF' },
	];

	/** Color codes used while creating an event */
	public static readonly audienceType: Array<IConstantDropdownItem> = [
		{ name: "Public", id: EventAudience.Public },
		{ name: "Private", id: EventAudience.Private },
	];

	/** Sort by values for filter */
	public static readonly sortBy: Array<IPostType> = [
		{ name: "Newest", id: SortBy.Recent, color: "" },
		{ name: "Popularity", id: SortBy.Popularity, color: "" }
	];

	/** Event type values */
	public static readonly eventType: Array<IConstantDropdownItem> = [
		{ name: "In person", id: EventType.InPerson },
		{ name: "Teams", id: EventType.Teams },
		{ name: "Live event", id: EventType.LiveEvent },
	];
}