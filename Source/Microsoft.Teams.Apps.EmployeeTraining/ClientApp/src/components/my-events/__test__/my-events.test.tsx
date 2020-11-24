import * as React from "react";
import MyEvents from "../my-events";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

jest.mock("../../../api/user-events-api");
jest.mock("../../../api/create-event-api");
jest.mock("../../../api/LnD-team-api");
jest.mock("../../../api/user-group-api");
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
                <MyEvents />
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

describe("MyEvents", () => {
    it("renders snapshots", () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("Tab Menu Index", async () => {
        const tabMenu = document.querySelector("[data-testid=tab_menu]");
        const allEventTab = tabMenu?.lastElementChild;
        await act(async () => {
            allEventTab?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventList = document.querySelector("[data-testid=event_count]");
        expect(eventList).toBe(null);
    });

    it("opens filter bar", () => {
        const filterButton = document.querySelector("[data-testid=filterbutton]");
        act(() => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const addFilterBar = document.querySelector("[data-testid=filterbar]");
        expect(addFilterBar).not.toBe(null);
    });

    it("filter Category", async () => {
        const filterButton = document.querySelector("[data-testid=filterbutton]");
        await act(async () => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const addFilterBar = document.querySelector("[data-testid=filterbar]");
        expect(addFilterBar).not.toBe(null);

        const categoryCheckBoxButton = document.querySelector(
            "[data-testid=categoryname1_CheckboxButton]"
        );
        await act(async () => {
            categoryCheckBoxButton?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const category1Checkbox = document.querySelector(
            "[data-testid=category1_categoryCheckbox_item]"
        );

        await act(async () => {
            category1Checkbox?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        expect(category1Checkbox?.getAttribute("aria-checked")).toBe("true");

        await act(async () => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
        const eventList = document.querySelector("[data-testid=event_count]");
        expect(eventList?.childElementCount).toBe(3);
    });

    it("filter CreatedBy", async () => {
        const filterButton = document.querySelector("[data-testid=filterbutton]");
        await act(async () => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const addFilterBar = document.querySelector("[data-testid=filterbar]");
        expect(addFilterBar).not.toBe(null);

        const createdByCheckBoxDropDown = document.querySelector(
            "[data-testid=user1_CheckboxButton]"
        );
        await act(async () => {
            createdByCheckBoxDropDown?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const createdByCheckbox = document.querySelector(
            "[data-testid=user1_categoryCheckbox_item]"
        );

        await act(async () => {
            createdByCheckbox?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        expect(createdByCheckbox?.getAttribute("aria-checked")).toBe("true");

        await act(async () => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const eventList = document.querySelector("[data-testid=event_count]");
        expect(eventList?.childElementCount).toBe(4);
    });

    it("filter SortBy", async () => {
        const filterButton = document.querySelector("[data-testid=filterbutton]");
        await act(async () => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        //expect(pretty(container.innerHTML)).toMatchInlineSnapshot();
        const addFilterBar = document.querySelector("[data-testid=filterbar]");
        expect(addFilterBar).not.toBe(null);

        const sortByDropDown = document.querySelector(
            "[data-testid=sortByNewest_RadioGroupButton]"
        );
        await act(async () => {
            sortByDropDown?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const radioButtons = document.querySelector(
            "[data-testid=radioGroup_items]"
        );

        const popularRadioButton = radioButtons?.lastChild;
        await act(async () => {
            popularRadioButton?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        await act(async () => {
            filterButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
        const eventList = document.querySelector("[data-testid=event_count]");

        expect(eventList?.childElementCount).toBe(2);
    });
});