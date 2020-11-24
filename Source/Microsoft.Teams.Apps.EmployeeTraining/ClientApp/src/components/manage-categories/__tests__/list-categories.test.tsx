import * as React from "react";
import ListCategories from "../list-categories";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
var pretty = require("pretty");

jest.mock("../../../api/manage-categories-api");
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
    initialize: () => { return true; },
    getContext: (callback: any) => callback(Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })),
}));

let container: any = null;

beforeAll(async () => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
    await act(async () => {
        render(
            <Provider>
                <ListCategories statusMessage="test message" onActionPerformed={() => { }} />
            </Provider>,
            container
        );
    });
});

afterAll(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe("ListCategories", () => {
    it("renders snapshots", async () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("selects all categories on click of 'select all' checkbox", async () => {
        const selectAllcheckbox = document.querySelector(
            "[data-testid=list-categories_selectAllCheckbox]"
        );
        act(() => {
            selectAllcheckbox?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
        expect(selectAllcheckbox?.getAttribute("aria-checked")).toBe("true");

        const checkbox = document.querySelector(
            "[data-testid=list-categories_categoryCheckbox0]"
        );
        expect(checkbox?.getAttribute("aria-checked")).toBe("true");
    });

    it("filters categories when search text is changed", async () => {
        let searchBar = document.querySelector(
            "[data-testid=searchbar]"
        );

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, "value")?.set;
            nativeInputValueSetter?.call(searchBar?.firstChild, "1");
            let ev = new Event('input', { bubbles: true });
            searchBar?.firstChild?.dispatchEvent(ev);
        });

        const table = document.querySelector(
            "[data-testid=table]"
        );

        expect(table?.childElementCount).toBe(2); // 1 category will be displayed in table after filtering. Other child node will be table header.
    });
});
