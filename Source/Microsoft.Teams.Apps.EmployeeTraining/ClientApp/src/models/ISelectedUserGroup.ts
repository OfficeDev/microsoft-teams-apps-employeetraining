// <copyright file="ISelectedUserGroup.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export interface ISelectedUserGroup {
    displayName: string,
    id: string,
    email: string,
    isGroup: boolean,
    isMandatory: boolean,
}