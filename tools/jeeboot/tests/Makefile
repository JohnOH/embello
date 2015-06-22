#Set this to @ to keep the makefile quiet
SILENCE = @
CPPUTEST_EXE_FLAGS = -c

ifeq ($(shell uname), Darwin)
CPPUTEST_CXXFLAGS=-Wno-c++11-extensions
else
CPPUTEST_CXXFLAGS=-std=c++11
endif

#---- Outputs ----#
COMPONENT_NAME = jeeboot

#--- Inputs ----#
PROJECT_HOME_DIR = ..

SRC_DIRS = $(PROJECT_HOME_DIR)/src

TEST_SRC_DIRS = .

INCLUDE_DIRS = \
	$(CPPUTEST_HOME)/include \
	$(PROJECT_HOME_DIR)/include \

include $(CPPUTEST_HOME)/build/MakefileWorker.mk
