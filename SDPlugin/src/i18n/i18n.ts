import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import * as resources from '../../assets/locales'

i18n
    .use(initReactI18next)
    .init({
        resources: resources,
        fallbackLng: "en",
    });

export default i18n;