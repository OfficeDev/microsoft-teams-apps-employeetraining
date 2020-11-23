// <copyright file="create-event-wrapper.test.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import CreateEventWrapper from "../create-event-wrapper";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

jest.mock("../../../api/common-api");
jest.mock("../../../api/user-group-api");
jest.mock("../../../api/create-event-api");
jest.mock("../../../helpers/event-helper");

jest.mock("react-i18next", () => ({
    useTranslation: () => ({
        t: (key: any) => key,
        i18n: { changeLanguage: jest.fn() },
    }),

    withTranslation: () => (Component: any) => {
        Component.defaultProps = {
            ...Component.defaultProps,
            t: (key: any) => key,
        };
        return Component;
    },
}));
jest.mock("@microsoft/teams-js", () => ({
    initialize: () => {
        return true;
    },
    getContext: (callback: any) =>
        callback(
            Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })
        ),
}));

let container: any = null;

beforeEach(() => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe("CreateEventWrapper", () => {
    it("renders snapshots", () => {
        act(() => {
            render(
                <Provider>
                    <CreateEventWrapper />
                </Provider>,
                container
            );
        });

        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });
});
