// <copyright file="create-event-step3.test.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import CreateEventStep3 from "../create-event-step3";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";
import TestData from "../../../api/test-data/test-data";

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
beforeEach(async () => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
    await act(async () => {
        render(
            <Provider>
                <CreateEventStep3
                    eventPageState={TestData.stateTest}
                    navigateToPage={(nextPage: any, stepEventState: any) => { }}
                />
            </Provider>,
            container
        );
    });
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe("CreateEventStep3", () => {
    it("renders snapshots", () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("CreateEventStep3 Create event button", async () => {
        const createEventButton = document.querySelector(
            "[data-testid=create_event_button]"
        );

        expect(createEventButton).not.toBe(null);
    });

    it("CreateEventStep3 Save event button", async () => {
        const saveEventButton = document.querySelector(
            "[data-testid=save_button]"
        );

        expect(saveEventButton).not.toBe(null);
    });

    it("CreateEventStep3 Update event button", async () => {

        act(() => {
            render(
                <Provider>
                    <CreateEventStep3
                        eventPageState={TestData.draftStateTest}
                        navigateToPage={(nextPage: any, stepEventState: any) => { }}
                    />
                </Provider>,
                container
            );
        });
        const updateEventButton = document.querySelector(
            "[data-testid=update_button]"
        );
        expect(updateEventButton).not.toBe(null);
    });
});
