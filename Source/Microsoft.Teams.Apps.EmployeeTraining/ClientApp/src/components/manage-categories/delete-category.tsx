// <copyright file="delete-category.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Table, Text, ExclamationCircleIcon } from "@fluentui/react-northstar";
import { ICategory } from "../../models/ICategory";
import { ResponseStatus } from "../../constants/constants";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { ManageCategoriesOperationFooter } from "./manage-categories-operation-footer";
import { deleteCategoriesAsync } from "../../api/manage-categories-api";
import withContext, { IWithContext } from "../../providers/context-provider";

interface IDeleteCategoryProps extends WithTranslation, IWithContext {
    categories: Array<ICategory>,
    onBackClicked: () => void,
    onCategoryDeleted: (message: string) => void
}

interface IDeleteCategoryState {
    isDeleting: boolean,
    errorMessage: string
}

/** This component manages the Delete category operations */
class DeleteCategory extends React.Component<IDeleteCategoryProps, IDeleteCategoryState> {
    readonly localize: TFunction = this.props.t;
    teamId: string;

    constructor(props: any) {
        super(props);

        this.teamId = "";

        this.state = {
            isDeleting: false,
            errorMessage: ""
        }
    }

    /**
     * Gets teams context from HOC as props
     */
    componentWillReceiveProps(nextProps: IDeleteCategoryProps) {
        if (this.props.teamsContext !== nextProps.teamsContext) {
            if (nextProps.teamsContext) {
                this.teamId = nextProps.teamsContext.teamId!;
            }
        }
    }

    /** The HTTP DELETE call to delete selected category/ies */
    deleteCategoryAsync = async () => {
        this.setState({ isDeleting: true, errorMessage: "" });

        let categoryIdsToDelete = this.props.categories.map((category) => {
            return category.categoryId;
        });

        let response = await deleteCategoriesAsync(this.teamId, categoryIdsToDelete.join(","));

        if (response.status === ResponseStatus.OK) {
            if (response.data === true) {
                this.props.onCategoryDeleted(this.localize("categoryDeleteSuccess"));
            }
            else {
                this.setState({ isDeleting: false, errorMessage: this.localize("failedToDeleteCategory") });
            }
        }
        else {
            this.setState({ isDeleting: false, errorMessage: this.localize("dataResponseFailedStatus") });
        }
    }

    /** Renders the categories that needs to be deleted */
    renderCategories = () => {
        if (this.props.categories && this.props.categories.length > 0) {
            const categoriesTableHeaderItems = {
                key: "header",
                items: [
                    { key: "category-name", className: "category-name-column", content: this.localize("categoryName") },
                    { key: "category-description", content: this.localize("description") }
                ]
            };

            let rows = this.props.categories.map((category: ICategory, index: number) => {
                return {
                    "key": index,
                    "items": [
                        {
                            className: "category-select-all-column",
                            content: category.isInUse ? <ExclamationCircleIcon title={this.localize("categoryInUse")} style={{ paddingLeft: ".42rem" }} /> : ""
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
                <Table
                    className="manage-categories-content categories-table manage-categories-content-background"
                    header={categoriesTableHeaderItems}
                    rows={rows}
                    data-testid="deletecategorytable"
                />
            );
        }
    }

    /** Renders component */
    render() {
        return (
            <React.Fragment>
                <Text content={this.localize("deleteCategoryConfirmation")} weight="semibold" />
                {this.renderCategories()}
                <ManageCategoriesOperationFooter
                    dir={this.props.dir}
                    backButtonContent={this.localize("back")}
                    submitButtonContent={this.localize("delete")}
                    isSubmitButtonDisabled={this.state.isDeleting}
                    isOperationInProgress={this.state.isDeleting}
                    errorMessage={this.state.errorMessage}
                    onBackClicked={this.props.onBackClicked}
                    onSubmit={this.deleteCategoryAsync}
                />
            </React.Fragment>
        );
    }
}

export default withTranslation()(withContext(DeleteCategory));