import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44334/',
  redirectUri: baseUrl,
  clientId: 'LegalTech_App',
  responseType: 'code',
  scope: 'offline_access LegalTech',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'LegalTech',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44334',
      rootNamespace: 'Acme.LegalTech',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
