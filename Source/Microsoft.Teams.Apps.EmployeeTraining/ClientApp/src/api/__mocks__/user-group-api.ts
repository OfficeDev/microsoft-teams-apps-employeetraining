// <copyright file="user-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ResponseStatus } from "../../constants/constants";
import { ISelectedUserGroup } from "../../models/ISelectedUserGroup";

/**
* Search users and groups.
*/
export const searchUsersAndGroups = async (searchText: string, searchDiretory: boolean): Promise<any> => {
    let groupMembers: Array<ISelectedUserGroup> = [
        {
            displayName: "Test1",
            id: "Test1",
            email: "test1@m.com",
            isGroup: true,
            isMandatory: true
        },
        {
            displayName: "Test2",
            id: "Test1",
            email: "test1@m.com",
            isGroup: true,
            isMandatory: true
        },
        {
            displayName: "Test3",
            id: "Test1",
            email: "test1@m.com",
            isGroup: true,
            isMandatory: true
        }
    ]
    return Promise.resolve({
        data: groupMembers,
        status: ResponseStatus.OK
    });
}

/**
* Get members of group.
*/
export const getGroupMembers = async (groupId: string): Promise<any> => {
    let groupMembers: Array<ISelectedUserGroup> = [
        {
            displayName: "Test1",
            id: "Test1",
            email: "test1@m.com",
            isGroup: true,
            isMandatory: true
        },
        {
            displayName: "Test2",
            id: "Test1",
            email: "test1@m.com",
            isGroup: true,
            isMandatory: true
        },
        {
            displayName: "Test3",
            id: "Test1",
            email: "test1@m.com",
            isGroup: true,
            isMandatory: true
        }
    ]
    return Promise.resolve({
        data: groupMembers,
        status: ResponseStatus.OK
    });
}

/**
 * Gets the user profiles
 * @param userIds The user IDs of which profiles to get
 */
export const getUserProfiles = async (userIds: Array<string>): Promise<any> => {
    return Promise.resolve({
        data: true,
        status: ResponseStatus.OK
    });
}