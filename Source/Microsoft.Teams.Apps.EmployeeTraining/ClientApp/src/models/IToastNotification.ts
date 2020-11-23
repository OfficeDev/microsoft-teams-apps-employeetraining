// <copyright file="IToastNotification.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ActivityStatus } from "./activity-status";

export interface IToastNotification {
    id: number
    message: string,
    type: ActivityStatus
}