import * as React from 'react';
import ManageCategories from '../manage-categories';
import { Provider } from '@fluentui/react-northstar';
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from 'pretty';

jest.mock('../../../api/manage-categories-api');
jest.mock('react-i18next', () => ({
    useTranslation: () => ({
        t: (key: any) => key,
        i18n: { changeLanguage: jest.fn() }
    }),
    withTranslation: () => (Component: any) => {
        Component.defaultProps = { ...Component.defaultProps, t: (key: any) => key };
        return Component;
    }
}));
jest.mock("@microsoft/teams-js", () => ({
    initialize: () => { return true; },
    getContext: (callback: any) => callback(Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })),
}));

let container: any = null;
beforeEach(() => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
    act(() => {
        render(<Provider><ManageCategories /></Provider>, container);
    });
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe('ManageCategories', () => {
    it('renders snapshots', () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it('opens add new category form', () => {
        const addButton = document.querySelector("[data-testid=addbutton]");

        act(() => {
            addButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const addForm = document.querySelector("[data-testid=addorupdateform]");
        expect(addForm).not.toBe(null);
    });

    it('navigates back to list of categories on "back" button click', () => {
        const backButton = document.querySelector("[data-testid=backbutton]");

        act(() => {
            backButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const addForm = document.querySelector("[data-testid=addorupdateform]");
        expect(addForm).toBe(null);
    });

    it('opens update category form', () => {
        const firstRowCheckbox = document.querySelector(
            "[data-testid=list-categories_categoryCheckbox0]"
        );
        act(() => {
            firstRowCheckbox?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        expect(firstRowCheckbox?.getAttribute("aria-checked")).toBe("true");

        const editButton = document.querySelector("[data-testid=editbutton]");

        act(() => {
            editButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const updateForm = document.querySelector("[data-testid=addorupdateform]");
        expect(updateForm).not.toBe(null);
    });

    it('opens delete categories confirmation', () => {
        const firstRowCheckbox = document.querySelector(
            "[data-testid=list-categories_categoryCheckbox0]"
        );
        act(() => {
            firstRowCheckbox?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        expect(firstRowCheckbox?.getAttribute("aria-checked")).toBe("true");

        const deleteButton = document.querySelector("[data-testid=deletebutton]");

        act(() => {
            deleteButton?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const deleteConfirmation = document.querySelector("[data-testid=deletecategorytable]");
        expect(deleteConfirmation).not.toBe(null);
    });
});