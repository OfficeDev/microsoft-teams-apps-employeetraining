// <copyright file="mandatory.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Label, RedbangIcon, Text, Layout } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

/** The mandatory event artifact */
const MandatoryArtifact: React.FunctionComponent<WithTranslation> = props => {
    const localize: TFunction = props.t;

    return (
        <Label
            circular
            className="event-artifact mandatory text-overflow-ellipsis"
            title={localize("mandatoryArtifactText")}
            content={
                <Layout className="text-overflow-ellipsis"
                    start={<RedbangIcon />}
                    main={<Text className="text-overflow-ellipsis" content={localize("mandatoryArtifactText")} size="small" weight="semibold" />}
                />
            }
        />
    );
}

export default withTranslation()(MandatoryArtifact);