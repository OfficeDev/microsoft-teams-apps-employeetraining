// <copyright file="ICheckBoxItem.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export interface ICheckBoxItem {
    key: string;
    title: string;
    checkboxLabel: JSX.Element,
    isChecked: boolean;
}