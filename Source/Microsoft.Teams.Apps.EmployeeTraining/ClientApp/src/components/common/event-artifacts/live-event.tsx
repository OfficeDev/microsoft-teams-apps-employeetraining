// <copyright file="live-event.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Label, BroadcastIcon, Layout } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

/** The live event artifact */
const LiveEventArtifact: React.FunctionComponent<WithTranslation> = props => {
    const localize: TFunction = props.t;

    return (
        <Label
            circular
            className="event-artifact"
            title={localize("liveEvent")}
            content={
                <Layout className="text-overflow-ellipsis"
                gap=".6rem"
                start={<BroadcastIcon />}
                main={<Text className="text-overflow-ellipsis" content={localize("liveEvent")} size="small" weight="semibold" />}
            />}
        />
    );
}

export default withTranslation()(LiveEventArtifact);