// <copyright file="add-update-category.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Input, Text, TextArea } from "@fluentui/react-northstar";
import { ICategory } from "../../models/ICategory";
import Constants, { ResponseStatus } from "../../constants/constants";
import { WithTranslation, withTranslation } from "react-i18next";
import withContext, { IWithContext } from "../../providers/context-provider";
import { TFunction } from "i18next";
import { ManageCategoriesOperationFooter } from "./manage-categories-operation-footer";
import { createCategoryAsync, updateCategoryAsync, getEventCategoriesAsync } from "../../api/manage-categories-api";

interface IAddCategoryProps extends WithTranslation, IWithContext {
    category?: ICategory
    onBackClicked: () => void,
    onCategoryAddedOrUpdated: (message: string) => void
}

interface IAddCategoryState {
    category: ICategory,
    isCreatingOrUpdating: boolean,
    errorMessage: string,
    isCategoryAlreadyExists: boolean
}

/** This component adds a new event category */
class AddUpdateCategory extends React.Component<IAddCategoryProps, IAddCategoryState> {
    readonly localize: TFunction;
    teamId: string;

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        this.teamId = "";

        let categoryDetails: ICategory;

        if (this.props.category) {
            categoryDetails = { ...this.props.category };
        }
        else {
            categoryDetails = {
                categoryId: "",
                name: "",
                description: "",
                isSelected: false,
                isInUse: false,
            };
        }

        this.state = {
            category: categoryDetails,
            isCreatingOrUpdating: false,
            errorMessage: "",
            isCategoryAlreadyExists: false
        };
    }

    /**
     * Gets teams context from HOC as props
     */
    componentWillReceiveProps(nextProps: IAddCategoryProps) {
        if (this.props.teamsContext !== nextProps.teamsContext) {
            if (nextProps.teamsContext) {
                this.teamId = nextProps.teamsContext.teamId!;

                if (this.props.category) {
                    this.setState({ category: { ...this.props.category } });
                }
            }
        }
    }

    /** The HTTP POST call to add a new category in storage */
    createOrUpdateCategoryAsync = async () => {
        this.setState({ isCreatingOrUpdating: true, errorMessage: "" });

        let isCategoryAlreadyExists: boolean = false;

        // Checking whether category name already exists-
        // - creating a new category
        // - updating a category after category name changed
        if (!this.props.category || (this.props.category.name !== this.state.category.name.trim())) {
            isCategoryAlreadyExists = await this.checkCategoryExists();

            if (isCategoryAlreadyExists) {
                this.setState({ isCategoryAlreadyExists: true, isCreatingOrUpdating: false });
                return;
            }
        }

        let response;

        if (this.props.category) {
            response = await updateCategoryAsync(this.teamId, this.state.category);

            if (response.status == ResponseStatus.OK) {
                if (response.data == true) {
                    this.props.onCategoryAddedOrUpdated(this.localize("categoryUpdateSuccess"));
                }
                else {
                    this.setState({ isCreatingOrUpdating: false, errorMessage: this.localize("failedToUpdateCategory") });
                }
            }
            else {
                this.setState({ isCreatingOrUpdating: false, errorMessage: this.localize("dataResponseFailedStatus") });
            }
        }
        else {
            response = await createCategoryAsync(this.teamId, this.state.category);

            if (response.status == ResponseStatus.OK && response.data === true) {
                this.setState({ isCreatingOrUpdating: false, errorMessage: "" });
                this.props.onCategoryAddedOrUpdated(this.localize("categoryAddedSuccess"));
            }
            else {
                this.setState({ isCreatingOrUpdating: false, errorMessage: this.localize("dataResponseFailedStatus") });
            }
        }
    }

    /** Checks whether a category with the same name already exists */
    checkCategoryExists = async () => {
        if (this.state.category.name?.length) {
            let result = await getEventCategoriesAsync();

            if (result && result.status === ResponseStatus.OK && result.data) {
                let categories: Array<ICategory> = result.data;

                if (!categories || categories.length === 0) {
                    return false;
                }

                let isCategoryAlreadyExists: boolean = categories.some((category: ICategory) => category.name.toLowerCase() === this.state.category.name.trim().toLowerCase());

                return isCategoryAlreadyExists;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }

    /**
     * The event handler called when category name changes
     * @param event The input event object
     */
    onNameChanged = (event: any) => {
        let category = this.state.category;
        category.name = event.target.value;

        this.setState({ category, isCategoryAlreadyExists: false });
    }

    /**
     * The event handler called when category description changes
     * @param event The input event object
     */
    onDescriptionChanged = (event: any) => {
        let category = this.state.category;
        category.description = event.target.value;

        this.setState({ category });
    }

    /** Renders the error message related to category name */
    renderCategoryNameError = () => {
        if (this.state.isCategoryAlreadyExists) {
            return (
                <Flex.Item push>
                    <Text error content={this.localize("categoryAlreadyExistsError")} weight="semibold" />
                </Flex.Item>
            );
        }
    }

    /** Renders a component */
    render() {
        return (
            <React.Fragment>
                <Flex column gap="gap.medium" data-testid="addorupdateform">
                    <Flex column gap="gap.smaller" >
                        <Flex space="between">
                            <Text content={`${this.localize("categoryName")}*`} weight="semibold" />
                            {this.renderCategoryNameError()}
                        </Flex>
                        <Input
                            fluid
                            required={true}
                            placeholder={this.localize("enterNamePlaceholder")}
                            value={this.state.category.name}
                            maxLength={Constants.categoryNameMaxLength}
                            onChange={this.onNameChanged}
                            data-testid="categoryname"
                        />
                    </Flex>
                    <Flex column gap="gap.smaller">
                        <Text content={`${this.localize("description")}*`} weight="semibold" />
                        <TextArea
                            className="add-update-category-description"
                            fluid
                            placeholder={this.localize("categoryDescriptionPlaceholder", { "categoryDescriptionMaxLength": Constants.categoryDescriptionMaxLength })}
                            maxLength={Constants.categoryDescriptionMaxLength}
                            value={this.state.category.description}
                            onChange={this.onDescriptionChanged}
                            data-testid="categorydescription"
                        />
                    </Flex>
                    <ManageCategoriesOperationFooter
                        dir={this.props.dir}
                        backButtonContent={this.localize("back")}
                        submitButtonContent={this.props.category ? this.localize("save") : this.localize("addCategory")}
                        isSubmitButtonDisabled={this.state.category.name.trim() === "" || this.state.category.description.trim() === "" || this.state.isCreatingOrUpdating || this.state.isCategoryAlreadyExists}
                        isOperationInProgress={this.state.isCreatingOrUpdating}
                        errorMessage={this.state.errorMessage}
                        onBackClicked={this.props.onBackClicked}
                        onSubmit={this.createOrUpdateCategoryAsync}
                    />
                </Flex>
            </React.Fragment>
        );
    }
}

export default withTranslation()(withContext(AddUpdateCategory));