// <copyright file="app.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
import * as React from "react";
import { AppRoute } from "./router/router";
import Resources from "./constants/constants";
import * as microsoftTeams from "@microsoft/teams-js";
import { Provider, ThemeInput, themes } from "@fluentui/react-northstar";
import { initializeIcons } from "@uifabric/icons";
import "./styles/style.css";

export interface IAppState {
    theme: string;
}

export default class App extends React.Component<{}, IAppState> {
    theme?: string | null = null;

    constructor(props: any) {
        super(props);
        let search = window.location.search;
        let params = new URLSearchParams(search);
        this.theme = params.get("theme");
        initializeIcons();

        this.state = {
            theme: this.theme ? this.theme : Resources.default,
        }
    }

    componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            this.setState({ theme: context.theme! });
        });
        microsoftTeams.registerOnThemeChangeHandler((theme: string) => {
            this.setState({ theme: theme! }, () => {
                this.forceUpdate();
            });
        });
    }

    setThemeComponent = () => {
        if (this.state.theme === Resources.dark) {
            return this.getAppDOM(themes.teamsDark, "dark-container");;
        }
        else if (this.state.theme === Resources.contrast) {
            return this.getAppDOM(themes.teamsHighContrast, "high-contrast-container");
        } else {
            return this.getAppDOM(themes.teams, "default-container");
        }
    }

    getAppDOM = (theme: ThemeInput | undefined, className: string) => {
        return (
            <Provider theme={theme}>
                <div className={className}>
                    <div className="appContainer">
                        <AppRoute />
                    </div>
                </div>
            </Provider>
        );
    }

    /**
    * Renders the component
    */
    public render(): JSX.Element {
        return (
            <div>
                {this.setThemeComponent()}
            </div>
        );
    }
}