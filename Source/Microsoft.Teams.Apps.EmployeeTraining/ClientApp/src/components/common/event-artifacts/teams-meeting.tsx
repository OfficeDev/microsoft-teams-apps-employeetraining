// <copyright file="teams-meeting.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Label, TeamsIcon, Layout } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

/** The teams-meeting artifact */
const TeamsMeetingArtifact: React.FunctionComponent<WithTranslation> = props => {
    const localize: TFunction = props.t;

    return (
        <Label
            circular
            className="event-artifact"
            title={localize("teamsMeeting")}
            content={<Layout className="text-overflow-ellipsis"
                gap=".6rem"
                start={<TeamsIcon />}
                main={<Text className="text-overflow-ellipsis" content={localize("teamsMeeting")} size="small" weight="semibold" />}
            />} />
    );
}

export default withTranslation()(TeamsMeetingArtifact);