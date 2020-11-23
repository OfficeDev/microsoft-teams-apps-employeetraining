/*
    <copyright file="constants.ts" company="Microsoft">
    Copyright (c) Microsoft. All rights reserved.
    </copyright>
*/

export default class Constants {
	//Themes
	public static readonly body: string = "body";
	public static readonly theme: string = "theme";
	public static readonly default: string = "default";
	public static readonly light: string = "light";
	public static readonly dark: string = "dark";
	public static readonly contrast: string = "contrast";

	//Constants for manage categories
	public static readonly categoryNameMaxLength: number = 100;
	public static readonly categoryDescriptionMaxLength: number = 300;

	public static readonly lazyLoadEventsCount: number = 50;

	public static readonly maxWidthForMobileView: number = 750;

	/** The base URL for API */
	public static readonly apiBaseURL = window.location.origin + "/api";
}

/** Indicates the operations that can be done on event categories */
export enum CategoryOperations {
	Add,
	Edit,
	Delete,
	Unknown
}

/** Indicates the response status codes */
export enum ResponseStatus {
	OK = 200
}