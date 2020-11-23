// <copyright file="LnD-Team-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { ResponseStatus } from "../../constants/constants";
import { ITeamsChannelMember } from "../../models/ITeamsChannelMember";

let baseAxiosUrl = window.location.origin + '/api';

/**
* Gets all LnD teams' members
*/
export const getAllLnDTeamMembersAsync = async (): Promise<any> => {
    let teamChannelMember: Array<ITeamsChannelMember> = [
        { aadObjectId: "user1", name: "user1" },
        { aadObjectId: "user2", name: "user2" },
        { aadObjectId: "user3", name: "user3" },
        { aadObjectId: "user4", name: "user4" }
    ]
    return Promise.resolve({
        data: teamChannelMember,
        status: ResponseStatus.OK
    });
}