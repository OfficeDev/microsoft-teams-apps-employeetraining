// <copyright file="audience.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { EyeIcon, EyeSlashIcon, Text, Label, Layout } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { EventAudience } from "../../../models/event-audience";
import withContext, { IWithContext } from "../../../providers/context-provider";
import { LanguageDirection } from "../../../models/language-direction";

interface IAudienceArtifact extends WithTranslation, IWithContext {
    audienceType: EventAudience
}

/** The event audience artifact */
const AudienceArtifact: React.FunctionComponent<IAudienceArtifact> = props => {
    const localize: TFunction = props.t;

    return (
        <Label
            circular
            className={props.dir === LanguageDirection.Rtl ? "event-artifact rtl-right-margin-small" : "event-artifact"}
            title={props.audienceType === EventAudience.Private ? localize("private") : localize("public")}
            content={
                <Layout className="text-overflow-ellipsis"
                gap=".6rem"
                start={props.audienceType === EventAudience.Private ? <EyeSlashIcon /> : <EyeIcon />}
                main={<Text className="text-overflow-ellipsis" content={props.audienceType === EventAudience.Private ? localize("private") : localize("public")} size="small" weight="semibold" />}
            />}
        />
    );
}

export default withTranslation()(withContext(AudienceArtifact));