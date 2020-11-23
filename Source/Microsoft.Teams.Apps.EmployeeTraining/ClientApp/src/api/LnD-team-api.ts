// <copyright file="LnD-team-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";

let baseAxiosUrl = window.location.origin + '/api';

/**
* Gets all LnD teams' members
*/
export const getAllLnDTeamMembersAsync = async (): Promise<any> => {
    let url = `${baseAxiosUrl}/LnDTeam`;
    return await axios.get(url);
}