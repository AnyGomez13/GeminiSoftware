import React from 'react';
import { Toaster } from 'sonner';
import { Sidebar } from './Sidebar';
import type { NavTab } from './Sidebar';
import { Header } from './Header';

interface MainLayoutProps {
  children: React.ReactNode;
  activeTab: NavTab;
  onTabChange: (tab: NavTab) => void;
  title: string;
  subtitle?: string;
  usuarioNombre?: string;
  onLogout?: () => void;
  recordatoriosPendientesCount?: number;
  searchValue?: string;
  onSearchChange?: (val: string) => void;
  onNewActionClick?: () => void;
}

export const MainLayout: React.FC<MainLayoutProps> = ({
  children,
  activeTab,
  onTabChange,
  title,
  subtitle,
  usuarioNombre,
  onLogout,
  recordatoriosPendientesCount,
  searchValue,
  onSearchChange,
  onNewActionClick,
}) => {
  return (
    <div className="flex h-screen w-screen overflow-hidden bg-botanical-linen">
      {/* Toast Notification Container (Sonner) */}
      <Toaster richColors position="top-right" closeButton />

      {/* Navigation Sidebar */}
      <Sidebar
        activeTab={activeTab}
        onTabChange={onTabChange}
        usuarioNombre={usuarioNombre}
        onLogout={onLogout}
        recordatoriosPendientesCount={recordatoriosPendientesCount}
      />

      {/* Main View Area */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <Header
          title={title}
          subtitle={subtitle}
          searchValue={searchValue}
          onSearchChange={onSearchChange}
          onNewPatientClick={onNewActionClick}
        />

        <main className="flex-1 overflow-y-auto p-6 bg-botanical-linen">
          <div className="max-w-7xl mx-auto space-y-6">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
};
