// <copyright file="create-event-step2.test.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>


import * as React from "react";
import CreateEventStep2 from "../create-event-step2";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";
import TestData from "../../../api/test-data/test-data";
import { LanguageDirection } from "../../../models/language-direction";

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
                <CreateEventStep2
                    eventPageState={TestData.stateTest}
                    navigateToPage={(nextPage: any, stepEventState: any) => { }}
                    dir={LanguageDirection.Ltr}
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

describe("CreateEventStep2", () => {
    it("renders snapshots", () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("CreateEventStep2 Private Audience", async () => {
        const audienceDropdown = document.querySelector(
            "[data-testid=event_audience_dropdown]"
        );
        const eventAudienceDropdownButton = audienceDropdown?.firstElementChild?.getElementsByTagName(
            "button"
        );

        await act(async () => {
            eventAudienceDropdownButton
                ?.item(0)
                ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventAudienceItems = audienceDropdown?.firstElementChild?.getElementsByTagName(
            "ul"
        );

        await act(async () => {
            eventAudienceItems
                ?.item(0)
                ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
        const eventPrivate = eventAudienceItems?.item(0)?.lastElementChild;
        await act(async () => {
            eventPrivate?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const audienceDropdownSearch = document.querySelector(
            "[data-testid=audience_dropdown_search]"
        );
        const autoRegisterToggleButton = document.querySelector(
            "[data-testid=auto_toggle]"
        );

        const mandatoryAllButton = document.querySelector(
            "[data-testid=audience_mandatory_button]"
        );

        await act(async () => {
            autoRegisterToggleButton?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const autoRegisterToggle = document.querySelector(
            "[data-testid=auto_toggle]"
        );
        await act(async () => {
            mandatoryAllButton?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        await act(async () => {
            audienceDropdownSearch?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const audienceDropdownSearchInput = audienceDropdownSearch?.firstElementChild?.getElementsByTagName("input")?.item(0);
        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(audienceDropdownSearchInput, "Random");
            let ev = new Event("input", { bubbles: true });
            audienceDropdownSearchInput?.dispatchEvent(ev);
        });


        expect(audienceDropdownSearch).not.toBe(null);
        expect(audienceDropdownSearchInput).not.toBe(null);
        expect(audienceDropdownSearchInput?.value).toEqual("Random");
        expect(autoRegisterToggleButton).not.toBe(null);
        expect(mandatoryAllButton).not.toBe(null);
        expect(autoRegisterToggle?.getAttribute("aria-checked")).toBe("true");
    });

    it("CreateEventStep2 Next button ", async () => {
        const nextButton = document.querySelector("[data-testid=next_button]");

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        expect(nextButton).not.toBe(null);
    });

    it("CreateEventStep2 Back button ", async () => {
        const backButton = document.querySelector("[data-testid=back_button]");

        await act(async () => {
            backButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        expect(backButton).not.toBe(null);
    });

    it("CreateEventStep2 Save button ", async () => {
        let eventState = TestData.draftStateTest;
        eventState.isEdit = true;
        eventState.isDraft = true;

        act(() => {
            render(
                <Provider>
                    <CreateEventStep2
                        eventPageState={eventState}
                        navigateToPage={(nextPage: any, stepEventState: any) => { }}
                        dir={LanguageDirection.Ltr}
                    />
                </Provider>,
                container
            );
        });

        const saveButton = document.querySelector(
            "[data-testid=save_draft_button]"
        );
        expect(saveButton).not.toBe(null);
    });
});
