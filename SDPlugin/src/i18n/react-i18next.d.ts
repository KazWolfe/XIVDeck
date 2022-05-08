import 'react-i18next';
import * as resources from '../../assets/locales'

declare module 'react-i18next' {
    interface CustomTypeOptions {
        defaultNS: 'common';
        resources: resources
    }
}