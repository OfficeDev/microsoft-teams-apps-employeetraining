// <copyright file="event-image.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, ImageUnavailableIcon } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

import "./event-image.css";

interface IEventImage extends WithTranslation {
    className: string | undefined,
    imageSrc: string
}

/**
 * Renders event image. If image not loaded successfully, then renders placeholder
 * @param props The props of type IEventImage
 */
const EventImage: React.FunctionComponent<IEventImage> = props => {
    let localize: TFunction = props.t;

    let [isImageLoaded, setImageLoaded] = React.useState(false);
    let [isErrorLoadingImage, setImageError] = React.useState(false);

    /** The event handler called when image loaded successfully */
    const onImageLoaded = () => {
        setImageLoaded(true);
    }

    /** The event handler called when image was not loaded or user aborted loading image */
    const onImageNotLoaded = () => {
        setImageError(true);
    }

    const renderImagePlaceholder = () => {
        if (!isImageLoaded && !isErrorLoadingImage) {
            return (
                <Flex
                    className={`${props.className} event-image-placeholder-container`}
                    vAlign="center"
                    hAlign="center">
                    <Flex className="event-image-placeholder" vAlign="center" hAlign="center" gap="gap.small" fill>
                        <Text
                            content={localize("eventCardImageLoadingPlaceholder")}
                            align="center" size="medium"
                            weight="semibold"
                            color="white"
                        />
                    </Flex>
                </Flex>
            );
        }
        else if (isErrorLoadingImage) {
            return (
                <Flex
                    className={`${props.className} event-image-placeholder-container`}
                    vAlign="center"
                    hAlign="center">
                    <Flex className="event-image-placeholder" vAlign="center" hAlign="center" gap="gap.small" fill>
                        <ImageUnavailableIcon className="placeholder-icon" />
                        <Text
                            content={localize("eventCardImageNotFoundPlaceholder")}
                            align="center" size="medium"
                            weight="semibold"
                            color="white"
                        />
                    </Flex>
                </Flex>
            );
        }
    }

    return (
        <React.Fragment>
            <img
                className={`${props.className} ${isImageLoaded && !isErrorLoadingImage ? 'event-image-renderer image-loaded' : 'event-image-renderer image-not-loaded'}`}
                src={props.imageSrc}
                onLoad={onImageLoaded}
                onError={onImageNotLoaded}
                onAbort={onImageNotLoaded} />
            {renderImagePlaceholder()}
        </React.Fragment>
    );
}

export default withTranslation()(EventImage);