import * as React from 'react';
import DeleteCategory from '../delete-category';
import { ICategory } from '../../../models/ICategory';
import renderer from 'react-test-renderer';
import { Provider } from '@fluentui/react-northstar';
import { shallow } from 'enzyme';
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
const categories: Array<ICategory> = [{ categoryId: "testid1", description: "category description", isInUse: false, isSelected: false, name: "category name", createdBy: "testuser", createdOn: new Date(), updatedBy: "testuser", updatedOn: new Date() }];

beforeAll(() => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
    act(() => {
        render(<Provider><DeleteCategory categories={categories} onBackClicked={() => { }} onCategoryDeleted={() => { }} /></Provider>, container);
    });
});

afterAll(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe('DeleteCategory', () => {
    it('renders snapshots', () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it('deletes categories on button click', () => {
        const button = document.querySelector("[data-testid=manage-categories-operation-footer_button]");
        expect(button?.firstElementChild?.innerHTML).toBe("delete");

        act(() => {
            button?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
    });
});