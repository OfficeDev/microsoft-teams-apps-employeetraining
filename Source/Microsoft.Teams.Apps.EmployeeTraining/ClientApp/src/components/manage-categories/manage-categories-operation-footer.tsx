// <copyright file="manage-categories-operation-footer.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Button, Flex, Text, ChevronStartIcon, ChevronEndIcon } from "@fluentui/react-northstar";
import { LanguageDirection } from "../../models/language-direction";

interface IManageCategoriesOperationFooter {
    backButtonContent: string,
    submitButtonContent: string,
    isSubmitButtonDisabled: boolean,
    isOperationInProgress: boolean,
    errorMessage: string,
    onBackClicked: () => void,
    onSubmit: () => void,
    dir: LanguageDirection
}

/**
 * The function component which renders footers for Add, Edit and Delete category operation
 * @param props The props of type IManageCategoriesOperationFooter
 */
export const ManageCategoriesOperationFooter: React.FunctionComponent<IManageCategoriesOperationFooter> = props => {
    /** Renders error if data operation fails */
    const renderError = () => {
        if (props.errorMessage && props.errorMessage !== "") {
            return (
                <Flex.Item push>
                    <Text error content={props.errorMessage} weight="semibold" />
                </Flex.Item>
            );
        }
    }

    return (
        <Flex className="manage-categories-footer" space="between">
            <Button data-testid="backbutton" className="back-button" text icon={props.dir === LanguageDirection.Rtl ? <ChevronEndIcon /> : <ChevronStartIcon />} content={props.backButtonContent} onClick={props.onBackClicked} />
            <Flex gap="gap.medium" vAlign="center" hAlign="center">
                {renderError()}
                <Flex.Item push>
                    <Button
                        primary
                        content={props.submitButtonContent}
                        disabled={props.isSubmitButtonDisabled}
                        loading={props.isOperationInProgress}
                        onClick={props.onSubmit}
                        data-testid="manage-categories-operation-footer_button"
                    />
                </Flex.Item>
            </Flex>
        </Flex>
    );
}