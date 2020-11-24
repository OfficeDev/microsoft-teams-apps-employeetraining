import * as React from "react";
import AddUpdateCategory from "../add-update-category";
import { ICategory } from "../../../models/ICategory";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

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
    initialize: () => {
        return true;
    },
    getContext: (callback: any) =>
        callback(
            Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })
        ),
}));

let container: any = null;
const categories: Array<ICategory> = [
    {
        categoryId: "testid1",
        description: "category description",
        isInUse: false,
        isSelected: false,
        name: "category name",
        createdBy: "testuser",
        createdOn: new Date(),
        updatedBy: "testuser",
        updatedOn: new Date(),
    },
];

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

describe("AddOrUpdateCategory", () => {
    it("renders snapshots", () => {
        act(() => {
            render(
                <Provider>
                    <AddUpdateCategory
                        category={undefined}
                        onBackClicked={() => { }}
                        onCategoryAddedOrUpdated={() => { }}
                    />
                </Provider>,
                container
            );
        });

        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("changes category name on input", () => {
        act(() => {
            render(
                <Provider>
                    <AddUpdateCategory
                        category={undefined}
                        onBackClicked={() => { }}
                        onCategoryAddedOrUpdated={() => { }}
                    />
                </Provider>,
                container
            );
        });
        const categoryNameInput = document.querySelector(
            "[data-testid=categoryname]"
        );
        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(categoryNameInput?.firstChild, "test name");
            let ev = new Event("input", { bubbles: true });
            categoryNameInput?.firstChild?.dispatchEvent(ev);
        });

        expect(categoryNameInput?.firstElementChild?.getAttribute("value")).toBe(
            "test name"
        );
    });

    it("creates new category on submit", () => {
        act(() => {
            render(
                <Provider>
                    <AddUpdateCategory
                        category={undefined}
                        onBackClicked={() => { }}
                        onCategoryAddedOrUpdated={() => { }}
                    />
                </Provider>,
                container
            );
        });
        const button = document.querySelector(
            "[data-testid=manage-categories-operation-footer_button]"
        );
        const categoryDescriptionInput = document.querySelector(
            "[data-testid=categorydescription]"
        );
        const categoryNameInput = document.querySelector(
            "[data-testid=categoryname]"
        );
        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(categoryNameInput?.firstChild, "test name");
            let ev = new Event("input", { bubbles: true });
            categoryNameInput?.firstChild?.dispatchEvent(ev);
        });

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLTextAreaElement.prototype,
                "value"
            )?.set;
            nativeInputValueSetter?.call(
                categoryDescriptionInput,
                "test description"
            );
            let ev = new Event("input", { bubbles: true });
            categoryDescriptionInput?.dispatchEvent(ev);
        });

        expect(button?.firstElementChild?.innerHTML).toBe("addCategory");
        expect(button?.getAttribute("aria-disabled")).toBe("false");

        act(() => {
            button?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
    });

    it("checks whether submit is allowed for empty category name", () => {
        act(() => {
            render(
                <Provider>
                    <AddUpdateCategory
                        category={undefined}
                        onBackClicked={() => { }}
                        onCategoryAddedOrUpdated={() => { }}
                    />
                </Provider>,
                container
            );
        });
        const categoryNameInput = document.querySelector(
            "[data-testid=categoryname]"
        );
        expect(categoryNameInput?.firstElementChild?.getAttribute("value")).toBe("");

        const button = document.querySelector(
            "[data-testid=manage-categories-operation-footer_button]"
        );
        expect(button?.getAttribute("aria-disabled")).toBe("true");
    });

    it("updates a category on submit", () => {
        act(() => {
            render(
                <Provider>
                    <AddUpdateCategory
                        category={categories[0]}
                        onBackClicked={() => { }}
                        onCategoryAddedOrUpdated={() => { }}
                    />
                </Provider>,
                container
            );
        });
        const categoryNameInput = document.querySelector(
            "[data-testid=categoryname]"
        );
        expect(categoryNameInput?.firstElementChild?.getAttribute("value")).toBe(
            "category name"
        );
        const button = document.querySelector(
            "[data-testid=manage-categories-operation-footer_button]"
        );
        expect(button?.firstElementChild?.innerHTML).toBe("save");
        expect(button?.getAttribute("aria-disabled")).toBe("false");
        act(() => {
            button?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
    });
});
