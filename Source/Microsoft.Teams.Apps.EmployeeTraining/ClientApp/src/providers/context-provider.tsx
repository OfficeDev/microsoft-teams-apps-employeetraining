// <copyright file="context-provider.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
import * as microsoftTeams from "@microsoft/teams-js";
import i18n from "i18next";
import React, { Component } from 'react';
import { LanguageDirection } from "../models/language-direction";

export interface IWithContext {
    teamsContext: microsoftTeams.Context | null,
    microsoftTeams: typeof microsoftTeams,
    dir: LanguageDirection
}

export default function withContext(WrappedComponent: any) {
    return class extends Component<any, IWithContext> {
        constructor(props:any) {
            super(props);
            this.state = {
                teamsContext: null,
                microsoftTeams: microsoftTeams,
                dir: LanguageDirection.Ltr
            };
        }

        componentDidMount() {
            microsoftTeams.initialize();
            microsoftTeams.getContext((context: microsoftTeams.Context) => {
                this.setState({ teamsContext: context, dir: i18n.dir(context.locale!) === LanguageDirection.Rtl ? LanguageDirection.Rtl : LanguageDirection.Ltr});
            });
        }

        /** Renders component */
        render() {
            return (
                <WrappedComponent {...this.props} teamsContext={this.state.teamsContext} microsoftTeams={this.state.microsoftTeams} dir={this.state.dir}/>
            );
        }
    }
}