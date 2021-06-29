// <copyright file="manage-categories.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Provider } from '@fluentui/react-northstar';
import { Fabric } from '@fluentui/react';
import ListCategories from "./list-categories";
import { CategoryOperations } from "../../constants/constants";
import AddUpdateCategory from './add-update-category';
import DeleteCategory from './delete-category';
import { ICategory } from "../../models/ICategory";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import withContext, { IWithContext } from "../../providers/context-provider";

import "./manage-categories.css";

interface IManageCategoriesProps extends WithTranslation, IWithContext {
}

interface IManageCategoriesState {
    operation: CategoryOperations,
    operationData: Array<ICategory>,
    operationStatusMessage: string
}

/** The class which manages manage-categories dialog content */
class ManageCategories extends React.Component<IManageCategoriesProps, IManageCategoriesState > {
    readonly localize: TFunction;

    /** Constructor which initializes state */
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;

        this.state = {
            operation: CategoryOperations.Unknown,
            operationData: [],
            operationStatusMessage: ""
        };
    }

    /** Sets operation whether it is Add, Edit or Delete. If 'Back' button pressed from any component,
    in that case the operation will be unknown.*/
    setCategoryOperation = (
        operationStatusMessage: string = "",
        categoryOperation: CategoryOperations = CategoryOperations.Unknown,
        operationData: Array<ICategory> = []) => {
        this.setState({ operation: categoryOperation, operationData, operationStatusMessage });
    }

    /** The event handler called when clicked on 'Back' from any category operation */
    onBackClicked = () => {
        this.setCategoryOperation();
    }

    /**
     * The event handler called when a category operation successfully executed
     * @param message The success message
     */
    onOperationSuccessful = (message: string) => {
        this.setCategoryOperation(message);
    }

    /**
     * An event handler gets called when clicked on Add, Edit and Delete category
     * @param categoryOperation The category operation performed
     * @param operationData The data that needs to be populated in selected category operation
     */
    onCategoryActionPerformed = (categoryOperation: CategoryOperations, operationData?: Array<ICategory>) => {
        this.setCategoryOperation("", categoryOperation, operationData);
    }

    /** Renders component based on Add, Edit or Delete */
    renderOperation = () => {
        switch (this.state.operation) {
            case CategoryOperations.Add:
                return <AddUpdateCategory
                    onBackClicked={this.onBackClicked}
                    onCategoryAddedOrUpdated={this.onOperationSuccessful} />;

            case CategoryOperations.Edit:
                return <AddUpdateCategory
                    category={this.state.operationData?.[0]}
                    onBackClicked={this.onBackClicked}
                    onCategoryAddedOrUpdated={this.onOperationSuccessful} />;

            case CategoryOperations.Delete:
                return <DeleteCategory
                    categories={this.state.operationData}
                    onBackClicked={this.onBackClicked}
                    onCategoryDeleted={this.onOperationSuccessful} />

            default:
                return <ListCategories statusMessage={this.state.operationStatusMessage} onActionPerformed={this.onCategoryActionPerformed} />;
        }
    }

    /** Renders the component */
    render() {
        return (
            <Fabric dir={this.props.dir}>
                <Provider>
                    <Flex>
                        <div className="task-module-container">
                            {this.renderOperation()}
                        </div>
                    </Flex>
                </Provider>
            </Fabric>
        );
    }
}

export default withTranslation()(withContext(ManageCategories));