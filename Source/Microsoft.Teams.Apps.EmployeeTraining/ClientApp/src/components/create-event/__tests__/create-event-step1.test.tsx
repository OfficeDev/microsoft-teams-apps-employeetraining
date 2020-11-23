// <copyright file="create-event-step1.test.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import CreateEventStep1 from "../create-event-step1";
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
                <CreateEventStep1
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

describe("CreateEventStep1", () => {
    it("renders snapshots", () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("CreateEventStep1 name validations", async () => {
        const eventNameInput = document.querySelector(
            "[data-testid=event_name_input]"
        );

        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventNameNullError = document.querySelector(
            "[data-testid=event_name_req_error]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(eventNameInput?.firstChild, "Random");
            let ev = new Event("input", { bubbles: true });
            eventNameInput?.firstChild?.dispatchEvent(ev);
        });

        const eventNameNoError = document.querySelector(
            "[data-testid=event_name_req_error]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(
                eventNameInput?.firstChild,
                TestData.dummyText(100)
            );
            let ev = new Event("input", { bubbles: true });
            eventNameInput?.firstChild?.dispatchEvent(ev);
        });

        const eventNameMaxError = document.querySelector(
            "[data-testid=event_name_max_error]"
        );

        expect(eventNameNullError).not.toBe(null);
        expect(eventNameNoError).toBe(null);
        expect(eventNameMaxError).not.toBe(null);
    });

    it("CreateEventStep1 photo validation", async () => {
        const eventNameInput = document.querySelector(
            "[data-testid=event_name_input]"
        );
        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventNameError = document.querySelector(
            "[data-testid=event_photo_error]"
        );

        expect(eventNameError).not.toBe(null);
    });

    it("CreateEventStep1 categories validation", async () => {
        const eventCategoryDropDown = document.querySelector(
            "[data-testid=event_category_dropdown]"
        );
        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventCategoryError = document.querySelector(
            "[data-testid=event_category_error]"
        );

        await act(async () => {
            eventCategoryDropDown?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });
        await act(async () => {
            eventCategoryDropDown?.firstElementChild?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });
        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventCategoryNoError = document.querySelector(
            "[data-testid=event_type_error]"
        );

        expect(eventCategoryError).not.toBe(null);
        expect(eventCategoryNoError).toBe(null);
    });

    it("CreateEventStep1 type in-person validations", async () => {
        const eventTypeDropdown = document.querySelector(
            "[data-testid=event_type_dropdown]"
        );

        const eventTypeDropdownButton = eventTypeDropdown?.firstElementChild?.getElementsByTagName(
            "button"
        );
        await act(async () => {
            eventTypeDropdownButton
                ?.item(0)
                ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventTypeItems = eventTypeDropdown?.firstElementChild?.getElementsByTagName(
            "ul"
        );

        await act(async () => {
            eventTypeItems
                ?.item(0)
                ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
        const eventTypeInPerson = eventTypeItems?.item(0)?.firstElementChild;
        await act(async () => {
            eventTypeInPerson?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const eventVenueInput = document.querySelector(
            "[data-testid=event_venue_input]"
        );

        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventVenueRequireError = document.querySelector(
            "[data-testid=event_venue_req_error]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(eventVenueInput?.firstChild, "Random");
            let ev = new Event("input", { bubbles: true });
            eventVenueInput?.firstChild?.dispatchEvent(ev);
        });

        const eventVenueNoError = document.querySelector(
            "[data-testid=event_venue_req_error]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(
                eventVenueInput?.firstChild,
                TestData.dummyText(100) + TestData.dummyText(100)
            );
            let ev = new Event("input", { bubbles: true });
            eventVenueInput?.firstChild?.dispatchEvent(ev);
        });

        const eventVenueMaxError = document.querySelector(
            "[data-testid=event_venue_max_error]"
        );

        expect(eventVenueInput).not.toBe(null);
        expect(eventVenueRequireError).not.toBe(null);
        expect(eventVenueNoError).toBe(null);
        expect(eventVenueMaxError).not.toBe(null);
    });

    it("CreateEventStep1 type live event validations", async () => {
        const eventTypeDropdown = document.querySelector(
            "[data-testid=event_type_dropdown]"
        );

        const eventTypeDropdownButton = eventTypeDropdown?.firstElementChild?.getElementsByTagName(
            "button"
        );
        await act(async () => {
            eventTypeDropdownButton
                ?.item(0)
                ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventTypeItems = eventTypeDropdown?.firstElementChild?.getElementsByTagName(
            "ul"
        );

        await act(async () => {
            eventTypeItems
                ?.item(0)
                ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
        const eventTypeLiveEvent = eventTypeItems?.item(0)?.lastElementChild;
        await act(async () => {
            eventTypeLiveEvent?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const eventLiveLinkInput = document.querySelector(
            "[data-testid=event_link_input]"
        );

        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventLinkRequireError = document.querySelector(
            "[data-testid=event_link_req_error]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(eventLiveLinkInput?.firstChild, "invalid");
            let ev = new Event("input", { bubbles: true });
            eventLiveLinkInput?.firstChild?.dispatchEvent(ev);
        });

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventInvalidError = document.querySelector(
            "[data-testid=event_link_req_error]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(
                eventLiveLinkInput?.firstChild,
                "https://www.google.com"
            );
            let ev = new Event("input", { bubbles: true });
            eventLiveLinkInput?.firstChild?.dispatchEvent(ev);
        });

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventLinkNoError = document.querySelector(
            "[data-testid=event_link_req_error]"
        );

        expect(eventLiveLinkInput).not.toBe(null);
        expect(eventLinkRequireError).not.toBe(null);
        expect(eventInvalidError).not.toBe(null);
        expect(eventLinkNoError).toBe(null);
    });

    it("CreateEventStep1 description validations", async () => {
        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );

        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventDescRequiredError = document.querySelector(
            "[data-testid=event_desc_req_error]"
        );

        expect(eventDescRequiredError).not.toBe(null);
    });

    it("Event max participants validations", async () => {
        const nextButton = document.querySelector(
            "[data-testid=event_next_button]"
        );
        await act(async () => {
            nextButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const maxParticipantsRequireError = document.querySelector(
            "[data-testid=event_max_req_error]"
        );

        const maxParticipantsInput = document.querySelector(
            "[data-testid=event_maxParticipants_input]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(maxParticipantsInput?.firstChild, "2");
            let ev = new Event("input", { bubbles: true });
            maxParticipantsInput?.firstChild?.dispatchEvent(ev);
        });

        const maxParticipantsNoError = document.querySelector(
            "[data-testid=event_max_req_error]"
        );

        expect(maxParticipantsRequireError).not.toBe(null);
        expect(maxParticipantsNoError).toBe(null);
    });

    it("CreateEventStep1 save as draft validations", async () => {
        const saveButton = document.querySelector(
            "[data-testid=event_save_button]"
        );
        await act(async () => {
            saveButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventNameInput = document.querySelector(
            "[data-testid=event_name_input]"
        );

        const eventNameNullError = document.querySelector(
            "[data-testid=event_name_req_error]"
        );

        expect(eventNameNullError).not.toBe(null);
    });

    it("CreateEventStep1 date picker", async () => {
        const datePicker = document.querySelector("[data-testid=event_datepicker]");
        const datePickerInput = datePicker?.getElementsByTagName("input").item(0);

        await act(() => {
            datePickerInput?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(datePickerInput, "OCT 23 2020");
            let ev = new Event("input", { bubbles: true });
            datePickerInput?.dispatchEvent(ev);
        });

        expect(datePickerInput?.value).toEqual("Fri Oct 23 2020");
    });
});
