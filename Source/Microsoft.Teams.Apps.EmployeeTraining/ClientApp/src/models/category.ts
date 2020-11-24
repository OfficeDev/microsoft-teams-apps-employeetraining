// <copyright file="category.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export interface ICategory {
    categoryId: string,
    name: string,
    description: string,
    createdBy?: string,
    createdOn?: Date,
    updatedBy?: string,
    updatedOn?: Date,
    isSelected: boolean,
    isInUse: boolean
}