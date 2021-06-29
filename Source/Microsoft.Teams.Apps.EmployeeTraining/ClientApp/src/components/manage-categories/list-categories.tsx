// <copyright file="list-categories.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Table, Flex, Input, Text, Button, Checkbox, Loader } from "@fluentui/react-northstar";
import { SearchIcon, AddIcon, EditIcon, TrashCanIcon, QuestionCircleIcon, PresenceAvailableIcon } from '@fluentui/react-icons-northstar';
import { CategoryOperations, ResponseStatus } from "../../constants/constants";
import { ICategory } from "../../models/ICategory";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import withContext, { IWithContext } from "../../providers/context-provider";
import { getCategoriesAsync } from "../../api/manage-categories-api";
import { LanguageDirection } from "../../models/language-direction";

interface IListCategoriesProps extends WithTranslation, IWithContext {
    statusMessage: string,
    onActionPerformed: (categoryOperation: CategoryOperations, operationData?: Array<ICategory>) => void
}

interface IListCategoriesState {
    isEditEnabled: boolean
    isDeleteEnabled: boolean,
    isAllCategoriesSelected: boolean,
    categories: Array<ICategory>,
    searchedCategories: Array<ICategory>,
    statusMessage: string,
    isLoading: boolean,
    showStatusIcon: boolean
}

/** This component lists all event categories */
class ListCategories extends React.Component<IListCategoriesProps, IListCategoriesState> {
    readonly localize: TFunction;
    searchText: string;

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;

        this.searchText = "";

        this.state = {
            isEditEnabled: false,
            isDeleteEnabled: false,
            isAllCategoriesSelected: false,
            categories: [],
            searchedCategories: [],
            statusMessage: this.props.statusMessage,
            isLoading: false,
            showStatusIcon: this.props.statusMessage !== "" && this.props.statusMessage.length > 0
        }
    }

    /** Gets called when component get mounted */
    componentDidMount() {
        this.setState({ isLoading: true });
    }

    /**
     * Gets teams context from HOC as props
     */
    componentWillReceiveProps(nextProps: IListCategoriesProps) {
        if (this.props.teamsContext !== nextProps.teamsContext) {
            if (nextProps.teamsContext) {
                this.getCategoriesAsync(nextProps.teamsContext.teamId!);
            }
        }
    }

    /**
     * Gets all event categories
     */
    getCategoriesAsync = async (teamId: string) => {
        let response = await getCategoriesAsync(teamId);

        if (response && response.status === ResponseStatus.OK) {
            let categories: Array<ICategory> = [];

            response.data?.forEach((category: ICategory) => {
                categories.push(category);
            });

            this.setState({categories, isLoading: false });
        }
        else {
            this.setState({ isLoading: false, statusMessage: this.localize("dataResponseFailedStatus") });
        }
    }

    /** Manages 'Edit' and 'Delete' button's enability and manages select all categories checked state */
    manageControlsEnabilityAndSelection() {
        let selectedCategoriesCount = this.state.categories.filter((category: ICategory) => { return category.isSelected })?.length;
        let statusMessage = "";

        let isAllCategoriesSelected = selectedCategoriesCount === this.state.categories.length;

        if (selectedCategoriesCount === 1) {
            statusMessage = this.localize("listCategoriesSelectedStatus",
                { "selectedCategoriesCount": selectedCategoriesCount, "totalCategoriesCount": this.state.categories.length });
            this.setState({ isEditEnabled: true, isDeleteEnabled: true, isAllCategoriesSelected, statusMessage, showStatusIcon: false });
        }
        else if (selectedCategoriesCount > 1) {
            statusMessage = this.localize("listCategoriesSelectedStatus",
                { "selectedCategoriesCount": selectedCategoriesCount, "totalCategoriesCount": this.state.categories.length });
            this.setState({ isEditEnabled: false, isDeleteEnabled: true, isAllCategoriesSelected, statusMessage, showStatusIcon: false });
        }
        else {
            this.setState({ isEditEnabled: false, isDeleteEnabled: false, isAllCategoriesSelected, statusMessage, showStatusIcon: false });
        }
    }

    getLastCategoryUpdateDateAndTime = (): any => {
        if (this.state.categories?.length > 0) {
            let categories = [...this.state.categories];

            categories.sort((category1, category2) => {
                if (category1.updatedOn && category2.updatedOn) {
                    return 1;
                }
                else if (!category1.updatedOn && category2.updatedOn) {
                    return 1;
                }
                else if (category1.updatedOn && !category2.updatedOn) {
                    return -1;
                }
                else {
                    return 0;
                }
            });

            this.localize("lastUpdatedOnStatus", { "lastUpdatedOnStatus": categories[categories.length - 1].updatedOn?.toUTCString });
        }
        else {
            return null;
        }
    }

    /**
     * Searches categories based on search text and display search results
     * @param seachText The seach text entered in seach box
     */
    searchCategories = (searchText: string) => {
        let categories: Array<ICategory> = this.state.categories ?? [];

        if (categories.length > 0 && searchText && searchText.trim() !== "") {
            this.searchText = searchText.trim();

            let searchedCategories = categories.filter((category) => {
                return category.name.toLowerCase().indexOf(this.searchText.toLowerCase()) > -1 ||
                    category.description.toLowerCase().indexOf(this.searchText.toLowerCase()) > -1;
            });

            this.setState({ searchedCategories });
        }
        else {
            this.searchText = "";
            this.setState({ categories, searchedCategories: [] });
        }
    }

    /**
     * The event handler for Add, Edit and Delete category
     * @param categoryOperation The category operation performed
     */
    onActionPerformed = (categoryOperation: CategoryOperations) => {
        if (categoryOperation === CategoryOperations.Edit || categoryOperation === CategoryOperations.Delete) {
            let categoryOperationData = this.state.categories.filter((category: ICategory) => category.isSelected);
            this.props.onActionPerformed(categoryOperation, categoryOperationData);
        }
        else {
            this.props.onActionPerformed(categoryOperation);
        }
    }

    /**
     * The event handler called when any category checked state changed
     * @param category The selected category details
     */
    onCategoryCheckedChange = (category: ICategory) => {
        category.isSelected = !category.isSelected;
        this.manageControlsEnabilityAndSelection();
    }

    /** The event handler called when select all categories checked state changed */
    onSelectAllCategoriesCheckedChange = () => {
        if (this.state.categories && this.state.categories.length > 0) {
            var categories = [...this.state.categories];

            for (let i = 0; i < categories.length; i++) {
                categories[i].isSelected = !this.state.isAllCategoriesSelected;
            }

            this.setState({ categories }, () => {
                this.manageControlsEnabilityAndSelection();
            });
        }
    }

    /**
     * The event handler called when seaching categories
     * @param event The input event object
     */
    onSearchTextChanged = (event: any) => {
        this.searchCategories(event.target.value);
    }

    /** Renders success icon if an category operation executed successfully */
    renderCategoryOperationStatusIcon = () => {
        if (this.state.showStatusIcon) {
            return <PresenceAvailableIcon className="success-icon" />;
        }
    }

    /** Renders footer */
    renderFooter = () => {
        if (this.state.statusMessage && this.state.statusMessage !== "") {
            return (
                <React.Fragment>
                    <Flex gap="gap.small" vAlign="center" hAlign="center">
                        {this.renderCategoryOperationStatusIcon()}
                        <Flex.Item grow>
                            <Text data-testid="statusmessage" content={this.state.statusMessage} weight="semibold" />
                        </Flex.Item>
                    </Flex>
                    <Flex.Item push>
                        <Text timestamp content={this.getLastCategoryUpdateDateAndTime()} weight="semibold" />
                    </Flex.Item>
                </React.Fragment>
            );
        }
    }

    /** Render categories */
    renderCategories = () => {
        if (this.state.isLoading) {
            return (
                <Flex className="task-module-loader" hAlign="center" vAlign="center" fill>
                    <Loader />
                </Flex>
            );
        }

        let categories = this.searchText !== "" ? this.state.searchedCategories : this.state.categories;

        if (categories?.length > 0) {
            const categoriesTableHeaderItems = {
                key: "header",
                items: [
                    { key: "select-all-categories", className: "category-select-all-column", content: <Checkbox data-testid="list-categories_selectAllCheckbox" key="categoriesTableHeader" checked={this.state.isAllCategoriesSelected} onChange={this.onSelectAllCategoriesCheckedChange} /> },
                    { key: "category-name", className: "category-name-column", content: this.localize("categoryName") },
                    { key: "category-description", content: this.localize("description") }
                ]
            };

            let rows = categories.map((category: ICategory, index: number) => {
                return {
                    "key": index,
                    "items": [
                        {
                            className: "category-select-all-column",
                            content: <Checkbox data-testid={"list-categories_categoryCheckbox" + index.toString()} key={index} checked={category.isSelected} onChange={() => this.onCategoryCheckedChange(category)} />
                        },
                        {
                            className: "category-name-column",
                            content: <Text content={category.name} weight="bold" />,
                            title: category.name,
                            truncateContent: true
                        },
                        {
                            content: category.description,
                            title: category.description,
                            truncateContent: true
                        }
                    ]}
            });

            return (
                <Table data-testid="table" className="manage-categories-content categories-table manage-categories-content-background"
                    header={categoriesTableHeaderItems}
                    rows={rows}
                />
            );
        }
        else {
            return (
                <Flex className="manage-categories-content" gap="gap.small">
                    <Flex.Item>
                        <div
                            style={{
                                position: "relative",
                            }}
                        >
                            <QuestionCircleIcon outline color="green" />
                        </div>
                    </Flex.Item>
                    <Flex.Item grow={this.props.dir === LanguageDirection.Ltr}>
                        <Flex column gap="gap.small" vAlign="stretch">
                            <div className={this.props.dir === LanguageDirection.Rtl ? "rtl-direction rtl-right-margin-small" : ""}>
                                <Text weight="bold" content={this.localize("categoriesNotAvailableHeader")} /><br />
                                <Text content={
                                    this.searchText !== "" ?
                                    this.localize("categoriesNotFoundForSearchedTextDescription", { searchedText: this.searchText }) :
                                    this.localize("categoriesNotAvailableHeaderDescription")}
                                />
                            </div>
                        </Flex>
                    </Flex.Item>
                </Flex>
            );
        }
    }

    /** Render search input based on culture direction. */
    renderSearchInput = () => {
            if (this.props.dir === LanguageDirection.Rtl)
            {
                return <div>
                    <Input
                        icon={<SearchIcon />}
                        iconPosition={"start"}
                        data-testid="searchbar"
                        placeholder={this.localize("searchPlaceholder")}
                        onChange={this.onSearchTextChanged}
                    />
                </div>
            }
            else if(this.props.dir === LanguageDirection.Ltr)
            {
                return <Input
                    icon={<SearchIcon />}
                    iconPosition={"end"}
                    data-testid="searchbar"
                    placeholder={this.localize("searchPlaceholder")}
                    onChange={this.onSearchTextChanged}
                />
            }
    }

    /** Renders the component */
    render() {
        return (
            <>
                <div className="commandbar-wrapper">
                    <Flex className="commandbar-wrapper-container">
                        <Button text data-testid="addbutton" className="list-categories-menu-button" icon={<AddIcon className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}/>} content={this.localize("add")} onClick={() => this.onActionPerformed(CategoryOperations.Add)} />
                        <Button text data-testid="editbutton" className="list-categories-menu-button" icon={<EditIcon className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}/>} disabled={!this.state.isEditEnabled} content={this.localize("edit")} onClick={() => this.onActionPerformed(CategoryOperations.Edit)} />
                        <Button text data-testid="deletebutton" icon={<TrashCanIcon className={this.props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}/>} disabled={!this.state.isDeleteEnabled} content={this.localize("delete")} onClick={() => this.onActionPerformed(CategoryOperations.Delete)} />
                        <Flex.Item push={this.props.dir === LanguageDirection.Ltr} grow={this.props.dir === LanguageDirection.Rtl} className="search-input-container">
                            {this.renderSearchInput()}
                        </Flex.Item>
                    </Flex>
                </div>
                { this.renderCategories() }
                <Flex className="manage-categories-footer" space="between">
                    { this.renderFooter() }
                </Flex>
            </>
        );
    }
}

export default withTranslation()(withContext(ListCategories));